namespace BDO
{
    public static class ComponentIdentifiers
    {
        private const string Prefix = "BDO.";
        // Widgets
        public static class Widgets
        {
            private const string WidgetPrefix = Prefix + "Widget.";

            public const string Button = WidgetPrefix + "Button";

            public const string ExternalNavigation = WidgetPrefix + "ExternalNavigation";

            public const string Imagewithtext = WidgetPrefix + "ImageWithText";

            public const string QuoteCard = WidgetPrefix + "QuoteCard";

            public const string TiledLink = WidgetPrefix + "TiledLinkSection";

            public const string Globallocation = WidgetPrefix + "ViewGlobalLocations";

            public const string SocialMediaButtons = WidgetPrefix + "SocialMediaButtons";

            public const string RichText = WidgetPrefix + "RichText";

            public const string AccordionWidget = WidgetPrefix + "AccordionWidget";

            public const string Form = "Kentico.FormWidget";

            public const string LocalNavigation = WidgetPrefix + "LocalNavigation";

            public const string KeyTakeaways = WidgetPrefix + "KeyTakeaways";

        }

        //sections
        public static class Sections
        {
            public const string DefaultSection = "Kentico.DefaultSection";

            private const string SectionPrefix = Prefix + "Section.";

            public const string Container = SectionPrefix + "Container";

            public const string TwoColumns = SectionPrefix + "TwoColumns";

            public const string OneThirdTwoThird = SectionPrefix + "OneThirdTwoThird";

            public const string TwoThirdOneThird = SectionPrefix + "TwoThirdOneThird";

            public const string ThreeColumns = SectionPrefix + "ThreeColumns";

            public const string FourColumns = SectionPrefix + "FourColumns";

            public const string FiveColumns = SectionPrefix + "FiveColumns";

            public const string LocalNavigation = SectionPrefix + "LocalNavigation";
        }
        public static class FormSections
        {
            public const string DefaultSection = "Kentico.DefaultSection";
            private const string FormSectionPrefix = Prefix + "FormSection.";
            public const string TwoColumns = FormSectionPrefix + "TwoColumns";
          
        }
        public static class FormComponents
        {
            private const string FormComponentPrefix = Prefix + "FormComponent.";

            public const string TextBoxReload = FormComponentPrefix + "TextBoxReload";

            public const string ConsentAgreement = FormComponentPrefix + "BDOConsentAgreement";

            public const string SecurityCode = FormComponentPrefix + "SecurityCode";

            public const string MonthYear = FormComponentPrefix + "MonthYear";

            public const string OrderedPageSelector = FormComponentPrefix + "OrderedPageSelector";
        }


    }
}
