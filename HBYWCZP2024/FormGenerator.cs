
using HBYWCZP2024.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024
{
    public static class FormGenerator
    {
        public static AutoFormGeneratorControl CreateForm(Type type, string title)
        {
            var control = new AutoFormGeneratorControl();
            control.Title = title;
            control.Fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => new ClassField { Name = x.Name, Value = Activator.CreateInstance(x.FieldType) })
                .ToList();
            return control;
        }
        /// <summary>
        /// 自动生成组件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static AutoFormGeneratorControl CreateForm(object obj, string title)
        {
            var control = new AutoFormGeneratorControl();
            control.Title = title;

            control.Fields = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => new ClassField { Name = x.Name, Value = x.GetValue(obj) })
                .ToList();
            return control;
        }

        public static T GetUpdatedObject<T>(AutoFormGeneratorControl form, T obj)
        {
            int a = 1;
            //var aa= __makeref(a);
            foreach (var field in form.Fields)
            {
                var prop = typeof(T).GetProperty(field.Name);
                if (prop != null)
                {
                    if (prop.PropertyType.IsEnum && field.Value is string)
                    {
                        object enumValue;
                        if (Enum.TryParse(prop.PropertyType, (string)field.Value, out enumValue))
                        {
                            prop.SetValue(obj, enumValue);
                        }
                        else
                        {
                            prop.SetValue(obj, Activator.CreateInstance(prop.PropertyType));
                        }
                    }
                    else
                    {
                        var type = prop.PropertyType;
                        if (type.IsGenericType)
                        {
                            type = type.GetGenericArguments()[0];
                        }
                        var cc = Convert.ChangeType(field.Value, type);
                        prop.SetValue(obj, cc);
                    }
                }


                //var cc = Convert.ChangeType(field.Value, prop.PropertyType);
                //if (prop != null) 
                //    prop.SetValue(obj, cc);
            }
            return obj;
        }
    }
}
