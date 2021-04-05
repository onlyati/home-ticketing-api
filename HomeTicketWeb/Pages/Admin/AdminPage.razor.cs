using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using HomeTicketWeb.Model;
using System.Text.Json.Serialization;

namespace HomeTicketWeb.Pages.Admin
{
    /*********************************************************************************************/
    /* This class is for admin page methods and properties                                       */
    /*********************************************************************************************/
    public partial class AdminPage : ComponentBase  
    {
        /*=======================================================================================*/
        /* Variables and objects                                                                 */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Parameters                                                                            */
        /*---------------------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------------------*/
        /* Get cascaded values                                                                   */
        /*---------------------------------------------------------------------------------------*/
        [CascadingParameter] public MainLayout Layout { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables and objects                                                  */
        /*---------------------------------------------------------------------------------------*/
        private AlertBox QuestionBox;

        private TreeMenu TMenu;                                                          // Reference for TreeMenu component
        private List<TreeMenuItem> UserMenu = new List<TreeMenuItem>()                   // Feed TreeMenu component with data, what menu is required
        {
            new TreeMenuItem() { Title = "User adjustment", Section = "User management", Id = 1 },
            new TreeMenuItem() { Title = "User-Group assigments", Section = "User management", Id = 2 },
            new TreeMenuItem() { Title = "System adjustment", Section = "Sytem management", Id = 3 },
            new TreeMenuItem() { Title = "Category adjustment", Section = "Category management", Id = 4 },
        };

        private NewUser AddUser = new NewUser();                                         // Model for EditForm
        private List<NewUser> Users;
        private NewUser ChangeInfo;

        private List<string> categoryList;
        private List<AssignCategory> userCategoryList;
        private string moveCategory;
        private List<SystemElem> systemList;
        private List<Model.User> userList;

        private NewSystem AddSystem = new NewSystem();
        private List<SystemElem> allSystem;
        private SystemElem ChangeSystemItem;

        private AssignCategory AddCategory = new AssignCategory();

        /*=======================================================================================*/
        /* Classes                                                                               */
        /*=======================================================================================*/
        private class NewSystem
        {
            [Required]
            public string SystemName { get; set; }
        }

        private class NewUser
        {
            [Required]
            [JsonPropertyName("username")]
            public string UserName { get; set; }

            [Required]
            [JsonPropertyName("password")]
            public string Password { get; set; }

            [Required]
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [Required]
            [JsonPropertyName("role")]
            public string Role { get; set; }
        }

        private class AssignCategory
        {
            [Required]
            public string SystemName { get; set; }

            [Required]
            public string CategoryName { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj as AssignCategory);
            }

            public bool Equals(AssignCategory other)
            {
                if (SystemName != other.SystemName)
                    return false;
                if (CategoryName != other.CategoryName)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(SystemName, CategoryName);
            }
        }

        /*=======================================================================================*/
        /* Methods                                                                               */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Function name: OnInitialized                                                          */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Check authority during component initailization. If failed, navigate the login screen */
        /*---------------------------------------------------------------------------------------*/
        protected override async Task OnInitializedAsync()
        {
            bool check = await RefreshService.RefreshToken(js, User, Configuration["ServerAddress"], Http, NavManager);
            if (!check)
                CloseWindow();

            StateHasChanged();

            if (check)
            {
                if (Layout != null)
                    if (Layout.Bar != null)
                        if (Layout.Bar.OpenedAppsCount() == 0)
                        {
                            NavManager.NavigateTo("/");
                            return;
                        }

                if (User.Role != UserRole.Admin)
                {
                    if (Layout != null)
                        if (Layout.AlertBox != null)
                            Layout.AlertBox.SetAlert("Insufficient access", "You are not an admin, there is nothing to see here", AlertBox.AlertBoxType.Info);

                    CloseWindow();
                }
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (TMenu.IsSelected(1))
            {
                await LoadUsers();
            }

            if(TMenu.IsSelected(2))
            {
                await LoadCategoryAssignment();
            }

            if(TMenu.IsSelected(3))
            {
                await LoadSystems();
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: LoadUsers                                                              */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This methof load the users and refresh the list which is displayed on the screen      */
        /*---------------------------------------------------------------------------------------*/
        public async Task LoadUsers()
        {
            User = null;
            var usersRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/user/list/all");
            if(usersRequest.StatusCode != HttpStatusCode.OK)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Error during loading", "Error occured during loading users", AlertBox.AlertBoxType.Error);
                return;
            }

            var tempUsers = JsonSerializer.Deserialize<List<Model.User>>(await usersRequest.Content.ReadAsStringAsync());

            Users = new List<NewUser>();
            foreach (var item in tempUsers)
            {
                Users.Add(new NewUser() { Email = item.Email, Role = item.Role, UserName = item.UserName, Password = "" });
            }

            Users = Users.OrderBy(s => s.Role).ThenBy(s => s.UserName).ToList();

            StateHasChanged();
        }


        /*---------------------------------------------------------------------------------------*/
        /* Function name: UpdateState                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Eventcallback for 'AfterClick' parameter of TreeMenu component. It runs after click   */
        /* has happened on an item in the menu                                                   */
        /*---------------------------------------------------------------------------------------*/
        public async Task UpdateState()
        {
            if (TMenu != null)
                if (TMenu.ActMenu != null)
                    AdminPageState.ActMenu = TMenu.ActMenu;

            if (TMenu.IsSelected(1))
            {
                await LoadUsers();
            }

            if (TMenu.IsSelected(2))
            {
                await LoadCategoryAssignment();
            }

            if(TMenu.IsSelected(3))
            {
                await LoadSystems();
            }

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: CloseWindow                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Eventcallback for 'WindowTitle' component. This defines what should happen if somebody*/
        /* click to the 'X' in the right corner                                                  */
        /*---------------------------------------------------------------------------------------*/
        private void CloseWindow()
        {
            if (Layout != null)
                if (Layout.Bar != null)
                    Layout.Bar.RemoveOpenedApp(NavManager.Uri.Substring(NavManager.BaseUri.Length - 1));
        }

        #region User adjustment
        /*---------------------------------------------------------------------------------------*/
        /* Function name: ChangeUserVerify                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called if admin wants to change a user. It gives a verification window */
        /* and if if it is verified it calls the change function                                 */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void ChangeUserVerify(string username, string password, string email, string role)
        {
            ChangeInfo = new NewUser()
            {
                Email = email,
                UserName = username,
                Password = password,
                Role = role,
            };

            if(QuestionBox != null)
                QuestionBox.SetAlert("Adjust user", "Are you sure you want change it?", AlertBox.AlertBoxType.Question, ChangeUser, ChangeUserCancel);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ChangeUserCancel                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called after admin cancelled the change.                               */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private async Task ChangeUserCancel()
        {
            ChangeInfo = null;
            await LoadUsers();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ChangeUser                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called after admin verified user change.                               */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private async Task ChangeUser()
        {
            Console.WriteLine("Change is starting...");

            var changeJson = JsonSerializer.Serialize<NewUser>(ChangeInfo);
            var changeRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/user/change/general", new StringContent(changeJson, Encoding.UTF8, "application/json"));
            if(changeRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await changeRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Adjust user", $"User change is failed: {badResponse.Message}", AlertBox.AlertBoxType.Warning);
                return;
            }

            Console.WriteLine("Change for role...");

            var promoteRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/user/change/role?username={ChangeInfo.UserName}&role={ChangeInfo.Role}", null);
            if(promoteRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await changeRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Adjust user", $"User change is failed: {badResponse.Message}", AlertBox.AlertBoxType.Warning);
                return;
            }

            Console.WriteLine("Change is done...");

            await LoadUsers();

            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust user", $"User change is done", AlertBox.AlertBoxType.Info);

            Console.WriteLine("Change is verified...");
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: DeleteUserVerify                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called if admin wantzs to remove a user, it gives a verification window*/
        /* and if it is verified it calls the delete user function                               */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void DeleteUserVerify(string username, string password, string email, string role)
        {
            ChangeInfo = new NewUser()
            {
                Email = email,
                UserName = username,
                Password = password,
                Role = role,
            };

            if (QuestionBox != null)
                QuestionBox.SetAlert("Adjust user", "Are you sure you want remove this user?", AlertBox.AlertBoxType.Question, DeleteUser, ChangeUserCancel);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: DeleteUser                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called if admin verified a user remove question                        */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private async Task DeleteUser()
        {
            var removeUserRequest = await Http.PostAsync($"{Configuration["ServerAddress"]}/user/remove?username={ChangeInfo.UserName}", null);
            if (removeUserRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await removeUserRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Delete user", $"User remove is failed: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            await LoadUsers();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AddUserSubmit                                                          */
        /*                                                                                       */
        /* Description:                                                                          */
        /* New user request has been submitted from the admin page.                              */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private async Task AddUserSubmit()
        {
            // Build the JSON
            var registerJson = JsonSerializer.Serialize<NewUser>(AddUser);
            var registerRequest = await Http.PostAsync($"{Configuration["ServerAddress"]}/user/register", new StringContent(registerJson, Encoding.UTF8, "application/json"));
            if(registerRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await registerRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Add user", $"Add user is failed: {badResponse.Message}", AlertBox.AlertBoxType.Warning);
                return;
            }

            if(AddUser.Role == UserRole.Admin.ToString())
            {
                var roleRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/user/change/role?username={AddUser.UserName}&role=admin", null);
                if(roleRequest.StatusCode != HttpStatusCode.OK)
                {
                    var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await registerRequest.Content.ReadAsStringAsync());
                    if (Layout != null)
                        if (Layout.AlertBox != null)
                            Layout.AlertBox.SetAlert("Add user", $"Promoting to admin is failed: {badResponse.Message}", AlertBox.AlertBoxType.Warning);
                    return;
                }
            }

            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Add user", "User is added", AlertBox.AlertBoxType.Info);

            await LoadUsers();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AddUserSubmitMissing                                                   */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called after admin verified user change, but it failed due to valida-  */
        /* tion                                                                                  */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void AddUserSubmitMissing()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust user", "Input field(s) is(are) missing", AlertBox.AlertBoxType.Error);
        }
        #endregion

        #region User-Category Assignment

        /*---------------------------------------------------------------------------------------*/
        /* Function name: LoadCategoryAssigment                                                  */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method load the lists based on what is selected                                  */
        /*---------------------------------------------------------------------------------------*/
        public async Task LoadCategoryAssignment()
        {
            /*-----------------------------------------------------------------------------------*/
            /* Load system list                                                                  */
            /*-----------------------------------------------------------------------------------*/
            // Load all categores
            var allSystemRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/system/list/all");
            if(allSystemRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await allSystemRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Load systems", $"Error occured during loading system: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            systemList = JsonSerializer.Deserialize<List<SystemElem>>(await allSystemRequest.Content.ReadAsStringAsync());
            systemList = systemList.OrderBy(s => s.Name).ToList();

            if (systemList.Count == 0)
                return;

            // If no system selected, set the first system
            if (string.IsNullOrEmpty(AdminPageState.AdminUsrCat.SelectedSystem) || string.IsNullOrWhiteSpace(AdminPageState.AdminUsrCat.SelectedSystem))
                AdminPageState.AdminUsrCat.SelectedSystem = systemList[0].Name;

            // Load categories
            var categoryRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/category/list/system?value={AdminPageState.AdminUsrCat.SelectedSystem}");
            if (categoryRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await categoryRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Load systems", $"Error occured during loading system: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            var allCategoryList = JsonSerializer.Deserialize<List<Category>>(await categoryRequest.Content.ReadAsStringAsync());
            categoryList = allCategoryList.Select(s => s.Name).OrderBy(s => s).ToList();

            /*-----------------------------------------------------------------------------------*/
            /* Load user list                                                                    */
            /*-----------------------------------------------------------------------------------*/
            // Load user list
            var allUserRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/user/list/all");
            if (allUserRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await allUserRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Load users", $"Error occured during loading users: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            userList = JsonSerializer.Deserialize<List<Model.User>>(await allUserRequest.Content.ReadAsStringAsync());
            userList = userList.OrderBy(s => s.UserName).ToList();
            if (userList.Count == 0)
                return;

            // If no user selected, set the first user
            if (string.IsNullOrEmpty(AdminPageState.AdminUsrCat.SelectedUser) || string.IsNullOrWhiteSpace(AdminPageState.AdminUsrCat.SelectedUser))
                AdminPageState.AdminUsrCat.SelectedUser = userList[0].UserName;

            // Load categories for user
            var userCatRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/category/list/user?value={AdminPageState.AdminUsrCat.SelectedUser}");
            if (userCatRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await userCatRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Load users", $"Error occured during loading users: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            var userCatList = JsonSerializer.Deserialize<List<Category>>(await userCatRequest.Content.ReadAsStringAsync());
            userCategoryList = userCatList.Select(s => new AssignCategory() { CategoryName = s.Name, SystemName = s.System.Name }).OrderBy(s => s.SystemName).ThenBy(s => s.CategoryName).ToList();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AssignSystemChanged                                                    */
        /*                                                                                       */
        /* Description:                                                                          */
        /* System select has been changed, needs to be updated the displayed categories.         */
        /*---------------------------------------------------------------------------------------*/
        public async Task AssignSystemChanged(ChangeEventArgs e)
        {
            // Save value into singletion
            AdminPageState.AdminUsrCat.SelectedSystem = e.Value.ToString();

            // Load categories
            var categoryRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/category/list/system?value={AdminPageState.AdminUsrCat.SelectedSystem}");
            if (categoryRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await categoryRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Load systems", $"Error occured during loading system: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            var allCategoryList = JsonSerializer.Deserialize<List<Category>>(await categoryRequest.Content.ReadAsStringAsync());
            categoryList = allCategoryList.Select(s => s.Name).OrderBy(s => s).ToList();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AssignUserChange                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* User select has been changed, needs to be updated the displayed categories.           */
        /*---------------------------------------------------------------------------------------*/
        public async Task AssignUserChanged(ChangeEventArgs e)
        {
            AdminPageState.AdminUsrCat.SelectedUser = e.Value.ToString();

            // Load categories for user
            var userCatRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/category/list/user?value={AdminPageState.AdminUsrCat.SelectedUser}");
            if (userCatRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await userCatRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Load users", $"Error occured during loading users: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            var userCatList = JsonSerializer.Deserialize<List<Category>>(await userCatRequest.Content.ReadAsStringAsync());
            userCategoryList = userCatList.Select(s => new AssignCategory() { CategoryName = s.Name, SystemName = s.System.Name }).OrderBy(s => s.SystemName).ThenBy(s => s.CategoryName).ToList();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AssignCategoryDrag                                                     */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Category element has been picked up.                                                  */
        /*---------------------------------------------------------------------------------------*/
        public void AssignCategoryDrag(DragEventArgs e, string item)
        {
            moveCategory = item;
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AssignCategoryDropped                                                  */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Categrory elem has been dropped, try to assign category for user                      */
        /*---------------------------------------------------------------------------------------*/
        public async Task AssignCategoryDropped(DragEventArgs e)
        {
            var checkExist = userCategoryList.Where(s => s.Equals(new AssignCategory() { CategoryName = moveCategory, SystemName = AdminPageState.AdminUsrCat.SelectedSystem })).FirstOrDefault();
            if(checkExist != null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Assign category", $"Category is already assigned", AlertBox.AlertBoxType.Warning);
                return;
            }

            if(AdminPageState.AdminUsrCat.SelectedUser != null)
            {
                var assignRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/category/assign?userName={AdminPageState.AdminUsrCat.SelectedUser}&categoryName={moveCategory}&systemName={AdminPageState.AdminUsrCat.SelectedSystem}", null);
                if(assignRequest.StatusCode != HttpStatusCode.OK)
                {
                    var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await assignRequest.Content.ReadAsStringAsync());
                    if (Layout != null)
                        if (Layout.AlertBox != null)
                            Layout.AlertBox.SetAlert("Assign category", $"Error occured during assign category: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                    return;
                }

                await LoadCategoryAssignment();
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: UnassignCategory                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* It runs if the selected category needs to be removed from the user                    */
        /*---------------------------------------------------------------------------------------*/
        public async Task UnassignCategory(string cat, string sys)
        {
            if(AdminPageState.AdminUsrCat.SelectedUser != null)
            {
                var assignRequest = await Http.PutAsync($"{Configuration["ServerAddress"]}/category/unassign?userName={AdminPageState.AdminUsrCat.SelectedUser}&categoryName={cat}&systemName={sys}", null);
                if (assignRequest.StatusCode != HttpStatusCode.OK)
                {
                    var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await assignRequest.Content.ReadAsStringAsync());
                    if (Layout != null)
                        if (Layout.AlertBox != null)
                            Layout.AlertBox.SetAlert("Unassign category", $"Error occured during unassign category: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                    return;
                }

                await LoadCategoryAssignment();
            }
        }
        #endregion

        #region System adjustment
        /*---------------------------------------------------------------------------------------*/
        /* Function name: LoadSystems                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Load all systems into a list if 'System Management' panel is displayed.               */
        /*---------------------------------------------------------------------------------------*/
        public async Task LoadSystems()
        {
            var allSystemRequest = await Http.GetAsync($"{Configuration["ServerAddress"]}/system/list/all");
            if (allSystemRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await allSystemRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unassign category", $"Error occured during unassign category: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            allSystem = JsonSerializer.Deserialize<List<SystemElem>>(await allSystemRequest.Content.ReadAsStringAsync());
            allSystem = allSystem.OrderBy(s => s.Name).ToList();

            StateHasChanged();
        }
        /*---------------------------------------------------------------------------------------*/
        /* Function name: AddSystemSubmit                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Editform is valid, and new category will try to be added                              */
        /*---------------------------------------------------------------------------------------*/
        public async Task AddSystemSubmit()
        {
            var addSystemRequest = await Http.PostAsync($"{Configuration["ServerAddress"]}/system/create?name={AddSystem.SystemName}", null);
            if (addSystemRequest.StatusCode != HttpStatusCode.OK)
            {
                var badResponse = JsonSerializer.Deserialize<GeneralMessage>(await addSystemRequest.Content.ReadAsStringAsync());
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unassign category", $"Error occured during unassign category: {badResponse.Message}", AlertBox.AlertBoxType.Error);
                return;
            }

            AddSystem = new NewSystem();
            await LoadSystems();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AddSystemSubmitMissing                                                 */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Editform is invalid                                                                   */
        /*---------------------------------------------------------------------------------------*/
        public void AddSystemSubmitMissing()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", $"Missing input parameters", AlertBox.AlertBoxType.Error);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void ChangeSystemVerify()
        {
            if(QuestionBox != null)
                QuestionBox.SetAlert("System adjustment", "Do you really want to change the system?", AlertBox.AlertBoxType.Question, ChangeSystem);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void ChangeSystem()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", "System is changed", AlertBox.AlertBoxType.Info);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void DeleteSystemVerify()
        {
            if(QuestionBox != null)
                QuestionBox.SetAlert("System adjustment", "Do you really want to delete the system?", AlertBox.AlertBoxType.Question, DeleteSystem);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void DeleteSystem()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", "System is deleted", AlertBox.AlertBoxType.Info);
        }

        #endregion

        #region Category adjustment
        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void AddCategorySubmit()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust category", $"New category ({AddCategory.SystemName}->{AddCategory.CategoryName}) has been added", AlertBox.AlertBoxType.Info);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void AddCategorySubmitMissing()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust category", "Missing input values", AlertBox.AlertBoxType.Error);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void ChangeCategoryVerify()
        {
            if(QuestionBox != null)
                QuestionBox.SetAlert("Adjust category", "Do you really want to change this record?", AlertBox.AlertBoxType.Question, ChangeCategory);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void ChangeCategory()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust category", "Category is changed", AlertBox.AlertBoxType.Info);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void DeleteCategoryVerify()
        {
            if(QuestionBox != null)
                QuestionBox.SetAlert("Adjust category", "Do you really want to delete this record?", AlertBox.AlertBoxType.Question, DeleteCategory);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name:                                                                        */
        /*                                                                                       */
        /* Description:                                                                          */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public void DeleteCategory()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust category", "Category is deleted", AlertBox.AlertBoxType.Info);
        }

        #endregion
    }
}
