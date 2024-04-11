using MvvmGen;

namespace ClipboardStudio.ViewModels
{
    [ViewModel]
    public partial class EntryViewModel
    {
        [Property]
        private int _id;

        [Property]
        private string _title;

        [Property]
        private string _text;

        public bool HasTitle
            => !string.IsNullOrEmpty(Title);
    }
}
