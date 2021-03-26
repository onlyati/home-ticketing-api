using HomeTicketWeb.Model;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Components
{
    /*********************************************************************************************/
    /* This class balongs to TreeMenu, it contains the variables, properties and methods         */
    /*********************************************************************************************/
    public partial class TreeMenu : ComponentBase
    {
        /*=======================================================================================*/
        /* Objects and variables                                                                 */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Parameters                                                                            */
        /*---------------------------------------------------------------------------------------*/
        [Parameter] public List<TreeMenuItem> MenuTree { get; set; }                     // List about the menu points
        [Parameter] public EventCallback AfterClick { get; set; }                        // Is there any extra after clikc?

        /*---------------------------------------------------------------------------------------*/
        /* Cascaded properties                                                                   */
        /*---------------------------------------------------------------------------------------*/
        [CascadingParameter] public TreeMenuItem ExternalActMenu { get; set; }

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables                                                              */
        /*---------------------------------------------------------------------------------------*/
        public TreeMenuItem ActMenu;                                                     // That menu elem which is actually active
        private List<List<TreeMenuItem>> SubMenu = new List<List<TreeMenuItem>>();       // Internal auxiliary variable

        /*=======================================================================================*/
        /* Methods                                                                               */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Function name: OnInitialized                                                          */
        /*                                                                                       */
        /* Description:                                                                          */
        /* When this component is called with a complex list, this method seprate them for 2 sim-*/
        /* ler list. If no active menu was provided, then the first menupoint is the default     */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        protected override void OnInitialized()
        {
            // Separate variables, list has list type: each list in the list contains only one section list
            int list_number = 0;
            for (int i = 0; i < MenuTree.Count; i++)
            {
                if (i == 0)
                {
                    SubMenu.Add(new List<TreeMenuItem>());
                    SubMenu[list_number].Add(MenuTree[i]);
                }
                else
                {
                    if (MenuTree[i].Section == MenuTree[i - 1].Section)
                    {
                        SubMenu[list_number].Add(MenuTree[i]);
                    }
                    else
                    {
                        SubMenu.Add(new List<TreeMenuItem>());
                        list_number++;
                        SubMenu[list_number].Add(MenuTree[i]);
                    }
                }
            }

            // Check that selected menu is pre-defined
            if (ExternalActMenu != null)
            {
                if (ExternalActMenu.Selected == false)
                {
                    MenuTree[0].Selected = true;
                }
                else
                {
                    foreach (var item in MenuTree)
                    {
                        if (item.Title == ExternalActMenu.Title)
                            item.Selected = true;
                    }
                }
            }

            if (AfterClick.HasDelegate)
                AfterClick.InvokeAsync();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: ChangeActive                                                           */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Method is for OnClick event                                                           */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        private void ChangeActive(List<TreeMenuItem> menu, TreeMenuItem selected)
        {
            foreach (var item in menu)
            {
                if (item.Selected)
                    item.Selected = false;

                if (item.Title == selected.Title)
                    item.Selected = true;
            }


            ActMenu = selected;

            if (AfterClick.HasDelegate)
                AfterClick.InvokeAsync();

            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: IsSelected                                                             */
        /*                                                                                       */
        /* Description:                                                                          */
        /* It tells that the specified menupoint is selected or not                              */
        /*                                                                                       */
        /*---------------------------------------------------------------------------------------*/
        public bool IsSelected(int id)
        {
            return MenuTree.Where(s => s.Id.Equals(id)).Select(s => s.Selected).FirstOrDefault();
        }
    }
}
