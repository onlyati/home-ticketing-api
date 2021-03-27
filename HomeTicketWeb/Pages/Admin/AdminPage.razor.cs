using HomeTicketWeb.Components;
using HomeTicketWeb.Model;
using HomeTicketWeb.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        private TreeMenu TMenu;                                                          // Reference for TreeMenu component
        private List<TreeMenuItem> UserMenu = new List<TreeMenuItem>()                   // Feed TreeMenu component with data, what menu is required
        {
            new TreeMenuItem() { Title = "User adjustment", Section = "User management", Id = 1 },
            new TreeMenuItem() { Title = "User-Group assigments", Section = "User management", Id = 2 },
            new TreeMenuItem() { Title = "System adjustment", Section = "Sytem management", Id = 3 },
            new TreeMenuItem() { Title = "Category adjustment", Section = "Category management", Id = 4 },
            new TreeMenuItem() { Title = "Category-System assignments", Section = "Category management", Id = 5 },
        };
        private NewUser AddUser = new NewUser();                                         // Model for EditForm

        private List<string> categoryList;
        private List<AssignCategory> userCategoryList;
        private string moveCategory;

        private NewSystem AddSystem = new NewSystem();

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
            public string UserName { get; set; }

            [Required]
            public string Password { get; set; }

            [Required]
            public string Email { get; set; }

            [Required]
            public string Role { get; set; }
        }

        private class AssignCategory
        {
            public string SystemName { get; set; }

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
        protected override void OnInitialized()
        {
            if (User.Role != UserRole.Admin || User.UserName == null)
            {
                if (Layout != null)
                    if (Layout.AlertBox != null)
                        Layout.AlertBox.SetAlert("Unathorized access", "You are not authorized. Login first if you want to do something", AlertBox.AlertBoxType.Error);

                if (Layout != null)
                    if (Layout.Bar != null)
                        Layout.Bar.RemoveOpenedApp("/admin");

                NavManager.NavigateTo("/");
            }
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: UpdateState                                                            */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Eventcallback for 'AfterClick' parameter of TreeMenu component. It runs after click   */
        /* has happened on an item in the menu                                                   */
        /*---------------------------------------------------------------------------------------*/
        public void UpdateState()
        {
            if (TMenu != null)
                if (TMenu.ActMenu != null)
                    AdminPageState.ActMenu = TMenu.ActMenu;

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
        private void ChangeUserVerify()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust user", "Are you sure you want change it?", AlertBox.AlertBoxType.Question, ChangeUser);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ChangeUser                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called after admin verified user change.                               */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void ChangeUser()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust user", "User change is done", AlertBox.AlertBoxType.Info);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: DeleteUserVerify                                                       */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called if admin wantzs to remove a user, it gives a verification window*/
        /* and if it is verified it calls the delete user function                               */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void DeleteUserVerify()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust user", "Are you sure you want remove this user?", AlertBox.AlertBoxType.Question, DeleteUser);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: DeleteUser                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method is called if admin verified a user remove question                        */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void DeleteUser()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust user", "User has been deleted", AlertBox.AlertBoxType.Info);
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: AddUserSubmit                                                          */
        /*                                                                                       */
        /* Description:                                                                          */
        /* New user request has been submitted from the admin page.                              */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void AddUserSubmit()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("Adjust user", "User is added", AlertBox.AlertBoxType.Info);
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
        public void AssignSystemChanged(ChangeEventArgs e)
        {
            AdminPageState.AdminUsrCat.SelectedSystem = e.Value.ToString();

            if(e.Value != null)
            {
                if (!string.IsNullOrEmpty(e.Value.ToString()))
                {
                    categoryList = new List<string>()
                    {
                        "System",
                        "Network",
                        "Storage",
                        "Application"
                    };
                }
                else
                    categoryList = null;
            }
            else
            {
                categoryList = null;
            }

            StateHasChanged();
        }

        public void AssignUserChanged(ChangeEventArgs e)
        {
            AdminPageState.AdminUsrCat.SelectedUser = e.Value.ToString();

            if(e.Value != null)
            {
                if(!string.IsNullOrEmpty(e.Value.ToString()))
                {
                    userCategoryList = new List<AssignCategory>()
                    {
                        new AssignCategory() {SystemName = "atihome", CategoryName = "Network" }
                    };
                }
                else
                {
                    userCategoryList = null;
                }
            }
            else
            {
                userCategoryList = null;
            }

            StateHasChanged();
        }

        public void AssignCategoryDrag(DragEventArgs e, string item)
        {
            moveCategory = item;
            
        }

        public void AssignCategoryDropped(DragEventArgs e)
        {
            if(AdminPageState.AdminUsrCat.SelectedUser != null)
            {
                var checkExist = userCategoryList.Where(s => s.CategoryName == moveCategory && s.SystemName == AdminPageState.AdminUsrCat.SelectedSystem).Select(s => s).FirstOrDefault();
                if(checkExist == null)
                {
                    userCategoryList.Add(new AssignCategory() { CategoryName = moveCategory, SystemName = AdminPageState.AdminUsrCat.SelectedSystem });
                    userCategoryList = userCategoryList.OrderBy(o => o.CategoryName).ToList();
                }
            }

            StateHasChanged();
        }

        public void UnassignCategory(string cat, string sys)
        {
            AssignCategory find = new AssignCategory()
            {
                CategoryName = cat,
                SystemName = sys,
            };

            var record = userCategoryList.Where(s => s.Equals(find)).FirstOrDefault();
            if(record != null)
            {
                userCategoryList.Remove(record);
            }

            StateHasChanged();
        }
        #endregion

        #region System adjustment

        public void AddSystemSubmit()
        {
            AddSystem = new NewSystem();
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", $"New system ({AddSystem.SystemName}) has been added", AlertBox.AlertBoxType.Info);
        }

        public void AddSystemSubmitMissing()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", $"Missing input parameters", AlertBox.AlertBoxType.Error);
        }

        public void ChangeSystemVerify()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", "Do you really want to change the system?", AlertBox.AlertBoxType.Question, ChangeSystem);
        }

        public void ChangeSystem()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", "System is changed", AlertBox.AlertBoxType.Info);
        }

        public void DeleteSystemVerify()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", "Do you really want to delete the system?", AlertBox.AlertBoxType.Question, DeleteSystem);
        }

        public void DeleteSystem()
        {
            if (Layout != null)
                if (Layout.AlertBox != null)
                    Layout.AlertBox.SetAlert("System adjustment", "System is deleted", AlertBox.AlertBoxType.Info);
        }

        #endregion
    }
}
