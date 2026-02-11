namespace Corsinvest.ProxmoxVE.Admin.Core.Validators;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class CronExpressionValidatorAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => CronHelper.IsValid(value + string.Empty);
}
