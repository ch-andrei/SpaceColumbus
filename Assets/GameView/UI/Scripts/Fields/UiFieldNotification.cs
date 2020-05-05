namespace UI.Fields
{
    public class UiFieldNotification : UiTwoTextField
    {
        public void Initialize(string time, string notification="")
        {
            this.textLeft.Text = time;

            if (notification.Length > 0)
                this.textRight.Text = notification;

            TriggerUpdateLayoutSize();
        }
    }
}