using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Attributes
{
    public class MultipleOfThreeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is int intValue)
            {
                return intValue % 3 == 0;
            }
            return false;
        }
    }
}
