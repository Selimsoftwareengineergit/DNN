namespace DNN.Models.ViewModels
{
    public class ManageUsersViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new List<UserListItemViewModel>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string Query { get; set; } = "";
    }
}