using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;

using MvcEnumDropDownList.Infrastructure;

namespace MvcEnumDropDownList.Helpers
{
	/// <summary>
	/// ASP.NET MVC - Creating a DropDownList helper for enums
	/// </summary>
	/// <see cref="http://blogs.msdn.com/b/stuartleeks/archive/2010/05/21/asp-net-mvc-creating-a-dropdownlist-helper-for-enums.aspx"/>
	public static partial class DropDownList
	{
		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
		{
			return EnumDropDownListFor(htmlHelper, expression, null /* optionLabel */, null /* htmlAttributes */);
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
		{
			return EnumDropDownListFor(htmlHelper, expression, null /* optionLabel */, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, IDictionary<string, object> htmlAttributes)
		{
			return EnumDropDownListFor(htmlHelper, expression, null /* optionLabel */, htmlAttributes);
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel)
		{
			return EnumDropDownListFor(htmlHelper, expression, optionLabel, null /* htmlAttributes */);
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel, object htmlAttributes)
		{
			return EnumDropDownListFor(htmlHelper, expression, optionLabel, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel, IDictionary<string, object> htmlAttributes)
		{
			if(expression == null)
				throw new ArgumentNullException("expression");

			string name = GetDropDownListName(htmlHelper, expression, htmlAttributes);

			IEnumerable<SelectListItem> selectList = htmlHelper.GetSelectListItemsFor<TModel, TEnum>(expression, null);
            SelectList realList = new SelectList(selectList, "Value","Text",selectList.Where(item=>item.Selected==true).Select(item=>item.Value).FirstOrDefault().ToString());
            MvcHtmlString htmlString = htmlHelper.DropDownList(name, selectList, optionLabel, htmlAttributes);

			return htmlString;
		}



		//
		//
		// Specifying Resource type
		//
		//

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum, TResource>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
		{
			return EnumDropDownListFor<TModel, TEnum, TResource>(htmlHelper, expression, null /* optionLabel */, null /* htmlAttributes */);
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum, TResource>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
		{
			return EnumDropDownListFor<TModel, TEnum, TResource>(htmlHelper, expression, null /* optionLabel */, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum, TResource>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, IDictionary<string, object> htmlAttributes)
		{
			return EnumDropDownListFor<TModel, TEnum, TResource>(htmlHelper, expression, null /* optionLabel */, htmlAttributes);
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum, TResource>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel)
		{
			return EnumDropDownListFor<TModel, TEnum, TResource>(htmlHelper, expression, optionLabel, null /* htmlAttributes */);
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum, TResource>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel, object htmlAttributes)
		{
			return EnumDropDownListFor<TModel, TEnum, TResource>(htmlHelper, expression, optionLabel, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}

		public static MvcHtmlString EnumDropDownListFor<TModel, TEnum, TResource>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel, IDictionary<string, object> htmlAttributes)
		{
			if(expression == null)
				throw new ArgumentNullException("expression");

			string name = GetDropDownListName(htmlHelper, expression, htmlAttributes);

			IEnumerable<SelectListItem> selectList = htmlHelper.GetSelectListItemsFor<TModel, TEnum>(expression, typeof(TResource));
			MvcHtmlString htmlString = htmlHelper.DropDownList(name, selectList, optionLabel, htmlAttributes);

			return htmlString;
		}


		#region Private methods 

		/// <summary>
		/// Generates the name attibute value for the DropDown
		/// </summary>
		/// <typeparam name="TModel">Model type</typeparam>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <param name="htmlHelper">HTML helper</param>
		/// <param name="expression">Model expression</param>
		/// <param name="htmlAttributes">HTML attributes</param>
		/// <returns></returns>
		public static string GetDropDownListName<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper,
			Expression<Func<TModel, TEnum>> expression, IDictionary<string, object> htmlAttributes = null)
		{
			string nameFromExpression = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));

			if(htmlAttributes == null)
				return nameFromExpression;

			// dropdownlist name:
			// if htmlAttributes["name"] exists, uses attribute 
			// if none of the previous attributes exists, uses name from lambda expression
			string name = htmlAttributes.ContainsKey("name") ?
				htmlAttributes["name"].ToString() :
				nameFromExpression;

			return name;
		}

		private static IEnumerable<SelectListItem> GetSelectListItemsFor<TModel, TEnum>(
			this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, Type resourceType)
		{
			bool isEnum = EnumHelper.IsEnum<TEnum>();

			if(!isEnum)
				throw new ArgumentException("TEnum must be an enumerated type");

			ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

			IEnumerable<SelectListItem> values = EnumHelper.GetEnumValues<TEnum>(resourceType).Select(x =>
				new SelectListItem {
					Text = x.Value,
					Value = Convert.ToInt32(x.Key).ToString(),
					Selected = x.Key.Equals(metadata.Model)
				}
			);

			return values;
		}

		#endregion
	}
}