//using Microsoft.ML;
//using Microsoft.ML.Data;

//var ml = new MLContext();

//var data = ml.Data.LoadFromTextFile<ModelInput>(
//    path: "data/moderation_vi_binary_clean.txt",
//    hasHeader: false,
//    separatorChar: '\t',
//    trimWhitespace: true);

//var pipeline = ml.Transforms.Text.FeaturizeText("Features", nameof(ModelInput.Text))
//    .Append(ml.BinaryClassification.Trainers.SdcaLogisticRegression(
//        labelColumnName: "Label",
//        featureColumnName: "Features"));

//var model = pipeline.Fit(data);

//ml.Model.Save(model, data.Schema, "model.zip");
//Console.WriteLine("✅ Training complete.");

//public class ModelInput
//{
//    [LoadColumn(0)] public string Text { get; set; } = "";
//    [LoadColumn(1)] public bool Label { get; set; } // 0 / 1 → bool
//}
