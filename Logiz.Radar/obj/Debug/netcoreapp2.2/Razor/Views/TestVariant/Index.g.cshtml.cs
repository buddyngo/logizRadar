#pragma checksum "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "ac3db446ba9e1f7bad0ba242e499f3fb88bb476e"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_TestVariant_Index), @"mvc.1.0.view", @"/Views/TestVariant/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/TestVariant/Index.cshtml", typeof(AspNetCore.Views_TestVariant_Index))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\_ViewImports.cshtml"
using Logiz.Radar;

#line default
#line hidden
#line 2 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\_ViewImports.cshtml"
using Logiz.Radar.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"ac3db446ba9e1f7bad0ba242e499f3fb88bb476e", @"/Views/TestVariant/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"4bd9ab9f3c124d9a3ac6c49bfeaf94eb72515492", @"/Views/_ViewImports.cshtml")]
    public class Views_TestVariant_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<IEnumerable<Logiz.Radar.Data.Model.TestVariant>>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Create", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-controller", "TestVariant", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Index", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("method", "post", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_4 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Edit", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_5 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("asp-action", "Delete", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(56, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 3 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
  
    ViewData["Title"] = "Index";

#line default
#line hidden
            BeginContext(99, 35, true);
            WriteLiteral("\r\n<h1>TestVariant</h1>\r\n\r\n<p>\r\n    ");
            EndContext();
            BeginContext(134, 37, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ac3db446ba9e1f7bad0ba242e499f3fb88bb476e5609", async() => {
                BeginContext(157, 10, true);
                WriteLiteral("Create New");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Action = (string)__tagHelperAttribute_0.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(171, 8, true);
            WriteLiteral("\r\n</p>\r\n");
            EndContext();
            BeginContext(179, 833, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ac3db446ba9e1f7bad0ba242e499f3fb88bb476e6992", async() => {
                BeginContext(247, 684, true);
                WriteLiteral(@"
    <!-- Input and Submit elements -->
    <div class=""row"">
        <div class=""col-md-4"">
            <div class=""form-group"">
                <label class=""control-label"">Project (*)</label>
                <select id=""ProjectID"" name=""ProjectID"" class=""form-control"" required>
                </select>
            </div>
            <div class=""form-group"">
                <label class=""control-label"">Scenario</label>
                <select id=""ScenarioID"" name=""ScenarioID"" class=""form-control"">
                </select>
            </div>
            <div class=""form-group"">
                <input type=""submit"" value=""Search"" class=""btn btn-primary"" /> | ");
                EndContext();
                BeginContext(932, 13, false);
#line 27 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                                                                            Write(Model.Count());

#line default
#line hidden
                EndContext();
                BeginContext(945, 60, true);
                WriteLiteral(" result(s)\r\n            </div>\r\n        </div>\r\n    </div>\r\n");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper.Controller = (string)__tagHelperAttribute_1.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_1);
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper.Action = (string)__tagHelperAttribute_2.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_2);
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper.Method = (string)__tagHelperAttribute_3.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_3);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(1012, 149, true);
            WriteLiteral("\r\n<table class=\"table\">\r\n    <thead>\r\n        <tr>\r\n            <th>\r\n                Scenario\r\n            </th>\r\n            <th>\r\n                ");
            EndContext();
            BeginContext(1162, 47, false);
#line 39 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
           Write(Html.DisplayNameFor(model => model.VariantName));

#line default
#line hidden
            EndContext();
            BeginContext(1209, 55, true);
            WriteLiteral("\r\n            </th>\r\n            <th>\r\n                ");
            EndContext();
            BeginContext(1265, 45, false);
#line 42 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
           Write(Html.DisplayNameFor(model => model.UpdatedBy));

#line default
#line hidden
            EndContext();
            BeginContext(1310, 55, true);
            WriteLiteral("\r\n            </th>\r\n            <th>\r\n                ");
            EndContext();
            BeginContext(1366, 51, false);
#line 45 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
           Write(Html.DisplayNameFor(model => model.UpdatedDateTime));

#line default
#line hidden
            EndContext();
            BeginContext(1417, 86, true);
            WriteLiteral("\r\n            </th>\r\n            <th></th>\r\n        </tr>\r\n    </thead>\r\n    <tbody>\r\n");
            EndContext();
#line 51 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
         foreach (var item in Model)
        {

#line default
#line hidden
            BeginContext(1552, 60, true);
            WriteLiteral("            <tr>\r\n                <td>\r\n                    ");
            EndContext();
            BeginContext(1613, 45, false);
#line 55 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
               Write(Html.DisplayFor(modelItem => item.ScenarioID));

#line default
#line hidden
            EndContext();
            BeginContext(1658, 67, true);
            WriteLiteral("\r\n                </td>\r\n                <td>\r\n                    ");
            EndContext();
            BeginContext(1726, 46, false);
#line 58 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
               Write(Html.DisplayFor(modelItem => item.VariantName));

#line default
#line hidden
            EndContext();
            BeginContext(1772, 67, true);
            WriteLiteral("\r\n                </td>\r\n                <td>\r\n                    ");
            EndContext();
            BeginContext(1840, 44, false);
#line 61 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
               Write(Html.DisplayFor(modelItem => item.UpdatedBy));

#line default
#line hidden
            EndContext();
            BeginContext(1884, 67, true);
            WriteLiteral("\r\n                </td>\r\n                <td>\r\n                    ");
            EndContext();
            BeginContext(1952, 50, false);
#line 64 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
               Write(Html.DisplayFor(modelItem => item.UpdatedDateTime));

#line default
#line hidden
            EndContext();
            BeginContext(2002, 47, true);
            WriteLiteral("\r\n                </td>\r\n                <td>\r\n");
            EndContext();
#line 67 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                     if (ViewBag.CanWrite)
                    {
                        

#line default
#line hidden
            BeginContext(2146, 30, true);
            WriteLiteral("\r\n                            ");
            EndContext();
            BeginContext(2176, 53, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ac3db446ba9e1f7bad0ba242e499f3fb88bb476e14077", async() => {
                BeginContext(2221, 4, true);
                WriteLiteral("Edit");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Action = (string)__tagHelperAttribute_4.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_4);
            if (__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.RouteValues == null)
            {
                throw new InvalidOperationException(InvalidTagHelperIndexerAssignment("asp-route-id", "Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper", "RouteValues"));
            }
            BeginWriteTagHelperAttribute();
#line 70 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                                                   WriteLiteral(item.ID);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.RouteValues["id"] = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-route-id", __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.RouteValues["id"], global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(2229, 32, true);
            WriteLiteral(" |\r\n                            ");
            EndContext();
            BeginContext(2261, 57, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("a", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "ac3db446ba9e1f7bad0ba242e499f3fb88bb476e16441", async() => {
                BeginContext(2308, 6, true);
                WriteLiteral("Delete");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.Action = (string)__tagHelperAttribute_5.Value;
            __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_5);
            if (__Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.RouteValues == null)
            {
                throw new InvalidOperationException(InvalidTagHelperIndexerAssignment("asp-route-id", "Microsoft.AspNetCore.Mvc.TagHelpers.AnchorTagHelper", "RouteValues"));
            }
            BeginWriteTagHelperAttribute();
#line 71 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                                                     WriteLiteral(item.ID);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.RouteValues["id"] = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("asp-route-id", __Microsoft_AspNetCore_Mvc_TagHelpers_AnchorTagHelper.RouteValues["id"], global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(2318, 26, true);
            WriteLiteral("\r\n                        ");
            EndContext();
#line 72 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                               
                    }

#line default
#line hidden
            BeginContext(2376, 42, true);
            WriteLiteral("                </td>\r\n            </tr>\r\n");
            EndContext();
#line 76 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
        }

#line default
#line hidden
            BeginContext(2429, 26, true);
            WriteLiteral("    </tbody>\r\n</table>\r\n\r\n");
            EndContext();
            DefineSection("Scripts", async() => {
                BeginContext(2473, 567, true);
                WriteLiteral(@"
    <script type=""text/javascript"">
        $(document).ready(function () {
            var url = rootUrl + ""Project/GetProjectSelectList"";
            $.getJSON(url, function (data) {
                var item = '<option value="""">- Select a project -</option>';
                $(""#ProjectID"").empty();
                $.each(data, function (i, project) {
                    item += '<option value=""' + project.value + '"">' + project.text + '</option>'
                });
                $(""#ProjectID"").html(item);
                $(""#ProjectID"").val('");
                EndContext();
                BeginContext(3041, 17, false);
#line 91 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                                Write(ViewBag.ProjectID);

#line default
#line hidden
                EndContext();
                BeginContext(3058, 793, true);
                WriteLiteral(@"').change();
            });
        });

        $(""#ProjectID"").change(function () {
            var projectId = $(""#ProjectID"").val();
            var url = rootUrl + ""TestScenario/GetTestScenarioSelectList"";
            $.getJSON(url, { ProjectId: projectId }, function (data) {
                var item = '<option value="""">- Select a scenario -</option>';
                var itemCodeList = [];
                $(""#ScenarioID"").empty();
                $.each(data, function (i, scenario) {
                    item += '<option value=""' + scenario.value + '"">' + scenario.text + '</option>'
                    itemCodeList[i] = scenario.value;
                });
                $(""#ScenarioID"").html(item);
                if (data.length > 0 && itemCodeList.includes('");
                EndContext();
                BeginContext(3852, 18, false);
#line 107 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                                                         Write(ViewBag.ScenarioID);

#line default
#line hidden
                EndContext();
                BeginContext(3870, 49, true);
                WriteLiteral("\')) {\r\n                    $(\"#ScenarioID\").val(\'");
                EndContext();
                BeginContext(3920, 18, false);
#line 108 "D:\workspace\Logiz.Radar\trunk\Logiz.Radar\Views\TestVariant\Index.cshtml"
                                     Write(ViewBag.ScenarioID);

#line default
#line hidden
                EndContext();
                BeginContext(3938, 69, true);
                WriteLiteral("\');\r\n                }\r\n            });\r\n        });\r\n    </script>\r\n");
                EndContext();
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IEnumerable<Logiz.Radar.Data.Model.TestVariant>> Html { get; private set; }
    }
}
#pragma warning restore 1591