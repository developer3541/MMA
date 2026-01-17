using System;
using WebApplication1.Models;

namespace WebApplication1.DTOs
{
    // ---------------- CoachProfile DTOs ----------------
    public class CreateCoachProfileDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? Certifications { get; set; }
    }

    public class UpdateCoachProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string LastName { get; set; }
        public string? Specialization { get; set; }
        public string? BlackBeltRanking { get; set; }
        public string? CoachingYears { get; set; }
    }
    public class CoachListDto
    {
        public int CoachId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public string? Certifications { get; set; }
        public string CoachingYears { get; set; }
        public string? BlackBeltRanking { get; set; }
    }
    public class MemberScheduleItemDto
    {
        public int SessionId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SessionName { get; set; }
        public string ClassType { get; set; }
        public string CoachName { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public AttendanceStatus? AttendanceStatus { get; set; }
        public string SessionState { get; set; } // Upcoming / Completed
    }
    public class MemberScheduleDayDto
    {
        public DateTime Date { get; set; }
        public List<MemberScheduleItemDto> Sessions { get; set; } = new();
    }
    public class MemberScheduleRequestDto
    {
        public int MemberId { get; set; }
        public int Month { get; set; } // 1 - 12
        public int Year { get; set; }
    }

    public class CoachProfileResponseDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string? Certifications { get; set; }
        public int SessionsCount { get; set; }
        public int FeedbacksCount { get; set; }
    }

    // ---------------- Session DTOs ----------------
    public class CreateSessionDto
    {
        public int CoachId { get; set; }
        public int ClassTypeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string? WhatToBring { get; set; }
        public string SessionName { get; set; } = string.Empty;  // تم إضافته
    }

    public class UpdateSessionDto
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? Capacity { get; set; }
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? WhattoBring { get; set; }
        public string? SessionName { get; set; }  // تم إضافته
    }

    public class SessionResponseDto
    {
        public int Id { get; set; }
        public int CoachId { get; set; }
        public string CoachName { get; set; } = string.Empty;
        public int ClassTypeId { get; set; }
        public string ClassTypeName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string SessionName { get; set; } = string.Empty;  // تم إضافته
        public int BookingsCount { get; set; }
        public int AttendanceCount { get; set; }
    }

    // ---------------- MemberSetProgress DTOs ----------------
    public class CreateMemberSetProgressDto
    {
        public int MemberId { get; set; }
        public DateTime Date { get; set; }
        public int SetsCompleted { get; set; }
        public DateTime? PromotionDate { get; set; }
    }

    public class UpdateMemberSetProgressDto
    {
        public int? SetsCompleted { get; set; }
        public DateTime? PromotionDate { get; set; }
    }

    public class MemberSetProgressResponseDto
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int SetsCompleted { get; set; }
        public DateTime? PromotionDate { get; set; }
    }

    // ---------------- Feedback DTOs ----------------
    public class CreateFeedbackDto
    {
        public int MemberId { get; set; }
        public int CoachId { get; set; }
        public int SessionId { get; set; }
        public decimal Rating { get; set; }
        public string? Comments { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class UpdateFeedbackDto
    {
        public decimal? Rating { get; set; }
        public string? Comments { get; set; }
    }

    public class FeedbackResponseDto
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public int CoachId { get; set; }
        public string CoachName { get; set; } = string.Empty;
        public int SessionId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public string? Comments { get; set; }
        public DateTime Timestamp { get; set; }
    }
    // ---------------- MemberProfile DTOs ----------------
    public class CreateMemberProfileDto
    {
        // كان UserId، الآن نستخدم UserName
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalInfo { get; set; }
        public DateTime JoinDate { get; set; }
    }

    public class UpdateMemberProfileDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        //public string? EmergencyContactPhone { get; set; }
        //public string? MedicalInfo { get; set; }
    }

    public class MemberProfileResponseDto
    {
        public int Id { get; set; }
        // نعرض UserName فقط
        public string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? MedicalInfo { get; set; }
        public DateTime JoinDate { get; set; }
        public int BookingsCount { get; set; }
        public int AttendanceCount { get; set; }
        public int FeedbacksGivenCount { get; set; }
        public int ProgressRecordsCount { get; set; }
    }

    // ---------------- ClassType DTOs ----------------
    public class CreateClassTypeDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class UpdateClassTypeDto
    {
        public string classid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class ClassTypeResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public int SessionsCount { get; set; }
    }

    // ---------------- Booking DTOs ----------------
    public class CreateBookingDto
    {
        public int SessionId { get; set; }
        public int MemberId { get; set; }
        public DateTime BookingTime { get; set; }
        public BookingStatus Status { get; set; }
    }

    public class UpdateBookingDto
    {
        public BookingStatus Status { get; set; }
    }

    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public string BookingStatus { get; set; }
        public DateTime BookingTime { get; set; }

        public int MemberId { get; set; }
        public string MemberName { get; set; }

        public SessionDto Session { get; set; }
    }

    public class SessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int EnrolledCount { get; set; }
        public int TotalSpots { get; set; }
        public string Status { get; set; }
        public string? Description { get; set; }
        public string? WhatToBring { get; set; }
        public string CoachName { get; set; }
        public string ClassTypeName { get; set; }
    }

    // ---------------- Attendance DTOs ----------------
    public class CreateAttendanceDto
    {
        public int SessionId { get; set; }
        public int MemberId { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public AttendanceStatus Status { get; set; }
    }

    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public AttendanceStatus Status { get; set; }
    }
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    public class VerifyOtpDto
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
    }
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }
    public class SessionParticipantDto
    {
        public int MemberId { get; set; }
        public string FullName { get; set; }
        public bool IsPresent { get; set; }
    }
    public class MarkAttendanceDto
    {
        public int SessionId { get; set; }

        // List of members marked PRESENT
        public List<int> PresentMemberIds { get; set; } = new();
    }
    public class MemberStatsDto
    {
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public int TotalSessionsAttended { get; set; }
        public int SessionsThisMonth { get; set; }
    }
    public class UpcomingSessionDto
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CoachName { get; set; }
        public string ClassType { get; set; }
    }
    public class LeaderboardDto
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public int TotalSessions { get; set; }
    }
    public class SessionListDto
    {
        public int Id { get; set; }
        public int CoachId { get; set; }
        public string CoachName { get; set; }
        public int ClassTypeId { get; set; }
        public string ClassTypeName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }
        public string SessionName { get; set; }
        public int BookingsCount { get; set; }
        public int AttendanceCount { get; set; }
    }
    public class CoachSessionDto
    {
        public string Id { get; set; }              // string for frontend compatibility
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int EnrolledCount { get; set; }
        public int TotalSpots { get; set; }
        public SessionStatus Status { get; set; }
        public string Description { get; set; }
        public string WhatToBring { get; set; }
    }


    public class MemberActivitySummaryDto
    {
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public double ConsistencyPercentage { get; set; }
        public List<ClassActivityBreakdownDto> ActivityBreakdown { get; set; }
        public List<MemberSessionDetailDto> Sessions { get; set; }
    }
    public class ClassActivityBreakdownDto
    {
        public int ClassTypeId { get; set; }
        public string ClassTypeName { get; set; }
        public int SessionsCount { get; set; }
    }
    public class MemberSessionDetailDto
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; }
        public string ClassType { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; } // Upcoming / Attended / Missed / Cancelled
    }
    public class MemberSessionDto   
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string CoachName { get; set; }
        public string CoachTitle { get; set; }

        public int TotalSpots { get; set; }
        public int AvailableSpots { get; set; }

        public string? Description { get; set; }
        public string? WhatToBring { get; set; }

        public bool IsBooked { get; set; }
    }

}