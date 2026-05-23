namespace TrainingHub.Security
{
    public static class RoleNames
    {
        public const string TrainingCoordinator = "Training Coordinator";
        public const string Instructor = "Instructor";
        public const string Trainee = "Trainee";

        public static readonly string[] All =
        [
            TrainingCoordinator,
            Instructor,
            Trainee
        ];
    }
}