using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Enum
{
    public enum RewardActionType
    {
        Login = 0,
        CompleteProfile = 1,
        SubmitAssignment = 2,
        ParticipateEvent = 3,
        CommentPost = 4,
        ReceiveLike = 5,
        CreatePost = 6,
        Custom = 99
    }
}
