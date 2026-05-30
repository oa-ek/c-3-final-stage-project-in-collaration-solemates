namespace StepStyle.Web.Models
{
    public enum Gender { Male, Female, Unisex, Kids }

    public enum OrderStatus
    {
        Pending,    
        Processing, 
        Shipped,    
        Delivered,  
        Cancelled   
    }

    public enum PaymentStatus
    {
        Unpaid,    
        Paid,       
        Refunded    
    }
}