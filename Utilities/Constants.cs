namespace PROG7312_POE.Utilities
{
    public static class Constants
    {
        public static class Categories
        {
            public const string Sanitation = "Sanitation";
            public const string Roads = "Roads";
            public const string Utilities = "Utilities";
            public const string Parks = "Parks and Recreation";
            public const string Other = "Other";

            public static readonly string[] AllCategories = 
            {
                Sanitation,
                Roads,
                Utilities,
                Parks,
                Other
            };
        }

        public static class Statuses
        {
            public const string Reported = "Reported";
            public const string InProgress = "In Progress";
            public const string Resolved = "Resolved";
            public const string Closed = "Closed";
        }

        public static class Messages
        {
            public const string IssueSubmitted = "Thank you for reporting the issue. Your reference number is: {0}";
            public const string ErrorOccurred = "An error occurred: {0}";
            public const string RequiredField = "This field is required";
            public const string FileTooLarge = "The selected file is too large. Maximum size is {0}MB";
        }

        public static class Settings
        {
            public const int MaxFileSizeMB = 5; // Maximum file size in MB
            public const string AppName = "Municipality Issue Tracker";
        }
    }
}
