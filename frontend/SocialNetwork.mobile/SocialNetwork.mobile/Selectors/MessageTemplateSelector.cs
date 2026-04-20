using System;
using Xamarin.Forms;
using SocialNetwork.mobile.DTO;

namespace SocialNetwork.mobile.Selectors
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LeftTemplate { get; set; }
        public DataTemplate RightTemplate { get; set; }
        public Guid CurrentUserId { get; set; } = Guid.Empty;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var msg = item as MessageDto;
            if (msg == null) return LeftTemplate;
            return msg.SenderId == CurrentUserId ? RightTemplate : LeftTemplate;
        }
    }
}