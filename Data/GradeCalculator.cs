using System;
using System.Collections.Generic;

namespace StudentReportInitial.Data
{
    public class GradeCalculator
    {
        // Component weights for each quarter
        private const double QUIZZES_ACTIVITIES_WEIGHT = 0.20;
        private const double PERFORMANCE_TASK_WEIGHT = 0.30;
        private const double EXAM_WEIGHT = 0.50;

        // Quarter weights for overall grade
        public const double PRELIM_WEIGHT = 0.20;
        public const double MIDTERM_WEIGHT = 0.20;
        public const double PREFINAL_WEIGHT = 0.20;
        public const double FINAL_WEIGHT = 0.40;

        public class QuarterGrade
        {
            public double QuizzesActivities { get; set; }
            public double PerformanceTask { get; set; }
            public double Exam { get; set; }

            public double CalculateQuarterGrade()
            {
                return (QuizzesActivities * QUIZZES_ACTIVITIES_WEIGHT) +
                       (PerformanceTask * PERFORMANCE_TASK_WEIGHT) +
                       (Exam * EXAM_WEIGHT);
            }
        }

        public static double CalculateOverallGrade(
            QuarterGrade prelim,
            QuarterGrade midterm,
            QuarterGrade preFinal,
            QuarterGrade final)
        {
            double prelimGrade = prelim.CalculateQuarterGrade();
            double midtermGrade = midterm.CalculateQuarterGrade();
            double preFinalGrade = preFinal.CalculateQuarterGrade();
            double finalGrade = final.CalculateQuarterGrade();

            double overallGrade = (prelimGrade * PRELIM_WEIGHT) +
                                 (midtermGrade * MIDTERM_WEIGHT) +
                                 (preFinalGrade * PREFINAL_WEIGHT) +
                                 (finalGrade * FINAL_WEIGHT);

            return overallGrade;
        }

        public static string GetLetterGrade(double grade)
        {
            if (grade >= 97.5) return "1.00 (Excellent)";
            if (grade >= 94.5) return "1.25";
            if (grade >= 91.5) return "1.50";
            if (grade >= 88.5) return "1.75";
            if (grade >= 85.5) return "2.00 (Very Good)";
            if (grade >= 82.5) return "2.25";
            if (grade >= 79.5) return "2.50";
            if (grade >= 76.5) return "2.75";
            if (grade >= 74.5) return "3.00 (Passed)";
            return "5.00 (Failed)";
        }

        public static double GetNumericGrade(double grade)
        {
            if (grade >= 97.5) return 1.00;
            if (grade >= 94.5) return 1.25;
            if (grade >= 91.5) return 1.50;
            if (grade >= 88.5) return 1.75;
            if (grade >= 85.5) return 2.00;
            if (grade >= 82.5) return 2.25;
            if (grade >= 79.5) return 2.50;
            if (grade >= 76.5) return 2.75;
            if (grade >= 74.5) return 3.00;
            return 5.00;
        }

        // GWA (General Weighted Average) calculation based on new ranges
        public static double GetGWANumericGrade(double grade)
        {
            if (grade >= 97.50) return 1.00;  // Excellent
            if (grade >= 94.50) return 1.25;   // Very Good
            if (grade >= 91.50) return 1.50;   // Very Good
            if (grade >= 86.50) return 1.75;   // Very Good
            if (grade >= 81.50) return 2.00;   // Satisfactory
            if (grade >= 76.00) return 2.25;   // Satisfactory
            if (grade >= 70.50) return 2.50;   // Satisfactory
            if (grade >= 65.00) return 2.75;   // Fair
            if (grade >= 59.50) return 3.00;   // Fair
            return 5.00;                        // Failed
        }

        public static string GetGWALetterGrade(double grade)
        {
            if (grade >= 97.50) return "1.00 (Excellent)";
            if (grade >= 94.50) return "1.25 (Very Good)";
            if (grade >= 91.50) return "1.50 (Very Good)";
            if (grade >= 86.50) return "1.75 (Very Good)";
            if (grade >= 81.50) return "2.00 (Satisfactory)";
            if (grade >= 76.00) return "2.25 (Satisfactory)";
            if (grade >= 70.50) return "2.50 (Satisfactory)";
            if (grade >= 65.00) return "2.75 (Fair)";
            if (grade >= 59.50) return "3.00 (Fair)";
            return "5.00 (Failed)";
        }
    }
}

