using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
namespace EduShpere.Application.Services
{
    public class ContentModerationService
    {
        private readonly PredictionEngine<PredictInput, ModelOutput> _engine;
        // Ngưỡng BLOCK (chặt)
        private const float BLOCK_THRESHOLD = 0.25f;

        // Các từ tục / gây hấn (dùng để xác định "lon" tục)
        private static readonly string[] Sexual = { "dit", "dm", "dmm", "dcm", "dcmm", "xoac", "chich", "hiep", "địt", "đụ" };
        private static readonly string[] Aggressive = { "may", "m", "tao", "thang", "con", "do", "cai" };
        private static readonly string[] ProfanityKeywords =
{
    "lồn", "l-ô-n", "l/ô/n",
    "dit me may", "dit", "me may", "dm", "dmm", "dcm", "dcmm",
    "djt", "duma", "cc", "ccmm",
    "lồn","lòn","lỏn","lỗn","lộn"
   ,"l0n","l*n","l@n","l+n","l=n",
    "l-ôn","l/ôn","l.ôn","l ôn","l o n", // dạng viết lách
    "lone","lonz","lonn","l0nz"
};
        public ContentModerationService(string modelPath)
        {
            var ml = new MLContext();
            var model = ml.Model.Load(modelPath, out _);
            _engine = ml.Model.CreatePredictionEngine<PredictInput, ModelOutput>(model);
        }
        public ModerationResult Check(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Clean();
            var rawWords = text
                .ToLower()
                .Normalize(NormalizationForm.FormD)
                .Replace("đ", "d")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (rawWords.Any(w => ProfanityKeywords.Contains(w)))
                return Block("keyword_block");
            if (IsLonInsult(text))
                return Block("context_lon");
            string normForModel = NormalizeForModel(text);
            var pred = _engine.Predict(new PredictInput { Text = normForModel });

            if (pred.Probability >= BLOCK_THRESHOLD)
                return Block("ml_block");

            return Clean();
        }
        private bool IsLonInsult(string raw)
        {
            string s = raw.ToLower();

            string[] safeContext =
            {
        "coca", "coca-cola", "bia", "sữa", "sua", "nước", "nuoc",
        "bò", "bo", "thùng", "thung", "lonbia", "loncoca", "hộp", "hop"
    };

            string[] sexual =
            {
        "địt", "dit", "đụ", "du", "xoạc", "chịch", "hiep"
    };

            string[] aggressive =
            {
        "m", "tao", "may", "thang", "con", "do", "dm", "dmm"
    };

            var words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (!words.Contains("lon"))
                return false;

            if (words.Any(w => safeContext.Contains(w)))
                return false;

            if (words.Any(w => sexual.Contains(w) || aggressive.Contains(w)))
                return true;

            return false;
        }
      

        private string NormalizeForModel(string s)
            => Regex.Replace(s.ToLower().Trim(), @"\s+", " ");

        private string NormalizeContext(string s)
        {
            s = s.ToLower();
            s = RemoveDiacritics(s);
            s = Regex.Replace(s, @"[^a-z0-9]+", ""); // loại bỏ "l/ồ/n", "l-o-n", "l o n"
            return s;
        }

        private string RemoveDiacritics(string text)
        {
            var form = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in form)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private ModerationResult Clean() =>
            new() { Decision = "clean", Reason = "ok" };

        private ModerationResult Block(string reason) =>
            new() { Decision = "block", Reason = reason };
        public class PredictInput
        {
            public string Text { get; set; } = "";
        }

        public class ModelOutput
        {
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }

            public float Score { get; set; }

            public float Probability { get; set; }
        }

    }

    public class ModerationResult
    {
        public string Decision { get; set; } = "clean"; // clean | block
        public string Reason { get; set; } = "ok";
    }
}
