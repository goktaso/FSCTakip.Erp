using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FSCTakip.WebUI.Binders
{
    /// <summary>
    /// Decimal form/query bağlamasını kültürden bağımsız hâle getirir.
    /// Sunucu OS kültürü tr-TR olduğunda "6000.0000" gibi JS nokta-ondalıkları
    /// binlik ayracı sanılıp 60000000'a dönüşüyordu. Bu binder önce InvariantCulture
    /// (nokta ondalık), olmazsa tr-TR (virgül ondalık) dener.
    /// </summary>
    public class InvariantDecimalModelBinder : IModelBinder
    {
        private static readonly CultureInfo Tr = new("tr-TR");

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == ValueProviderResult.None)
                return Task.CompletedTask;

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);
            var raw = valueResult.FirstValue;
            var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;

            if (string.IsNullOrWhiteSpace(raw))
            {
                if (isNullable)
                    bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var val)
                || decimal.TryParse(raw, NumberStyles.Number, Tr, out val))
            {
                bindingContext.Result = ModelBindingResult.Success(val);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Geçersiz sayı formatı.");
            }

            return Task.CompletedTask;
        }
    }

    public class InvariantDecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var type = context.Metadata.ModelType;
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying == typeof(decimal) ? new InvariantDecimalModelBinder() : null;
        }
    }
}
