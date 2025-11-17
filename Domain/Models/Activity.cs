using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Activity : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }


    [StringLength(20)]
    public string? Location { get; set; }

    public int? ClubId { get; set; }

    public string Organizer { get; set; } = null!;
    public int MaxParticipants { get; set; }
    public DateTime RegisterDate { get; set; }
    public DateTime EndRegisterDate { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public ActivityType Category { get; set; }
    public string SubType { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public bool? IsGrade { get; set; } = false; // Enable grading for this activity (nullable to handle NULL in database)
    public string? GradingSettings { get; set; } // JSON string for grading criteria (only criteria, not enabled flag)
    public bool? OnlyTeacherCanRegister { get; set; } = false; // Only teachers can register for this activity (nullable to handle NULL in database)

    [InverseProperty("Activity")]
    public virtual ICollection<ActivityParticipant> ActivityParticipants { get; set; } = new List<ActivityParticipant>();

    [InverseProperty("Activity")]
    public virtual ICollection<ActivityReward> ActivityRewards { get; set; } = new List<ActivityReward>();

    [ForeignKey("ClubId")]
    [InverseProperty("Activities")]
    public virtual Club? Club { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Activities")]
    public virtual User CreatedByUser { get; set; } = null!;

    [InverseProperty("Activity")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    [InverseProperty("Activity")]
    public virtual ICollection<ActivityRule> Rules { get; set; } = new List<ActivityRule>();
    public virtual ICollection<JuryActivity> JuryActivities { get; set; }
    
    [InverseProperty("Activity")]
    public virtual ICollection<ActivitySpeaker> Speakers { get; set; } = new List<ActivitySpeaker>();
    
    [InverseProperty("Activity")]
    public virtual ICollection<ActivityProgram> Programs { get; set; } = new List<ActivityProgram>();
    
    [InverseProperty("Activity")]
    public virtual ICollection<ActivitySport> Sports { get; set; } = new List<ActivitySport>();
    
    [InverseProperty("Activity")]
    public virtual ActivityDetail? ActivityDetail { get; set; }
    
    [InverseProperty("Activity")]
    public virtual ActivityRegistrationReward? RegistrationReward { get; set; }

    [InverseProperty("Activity")]
    public virtual ICollection<ActivityMatch> ActivityMatches { get; set; } = new List<ActivityMatch>();
}
