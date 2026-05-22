namespace CanteenSystem.Domain.Enums;

/// <summary>User access roles</summary>
public enum UserRole
{
    User = 0,
    Admin = 1,
    SuperAdmin = 2   // Reserved for future use
}

public enum OtpType
{
    EmailVerification = 0,
    PasswordReset = 1
}

public enum MealSlot
{
    Breakfast = 0,
    Lunch = 1,
    Snacks = 2,
    Dinner = 3 // Reserved for future use
}

public enum OrderStatus
{
    Pending = 0,      
    Confirmed = 1,    
    Preparing = 2,    
    Ready = 3,        
    Collected = 4,    
    Cancelled = 5     
}

public enum PaymentStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2,
    Refunded = 3
}

public enum PaymentMethod
{
    Cash = 0,
    UPI = 1, // Reserved for future use
    Card = 2, 
    NetBanking = 3 // Reserved for future use
}

public enum NotificationType
{
    OrderStatus = 0,
    PaymentSuccess = 1,
    PaymentFailed = 2,
    OrderReady = 3,
    System = 4
}