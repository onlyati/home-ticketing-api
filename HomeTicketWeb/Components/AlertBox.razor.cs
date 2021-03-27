using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeTicketWeb.Components
{
    /*********************************************************************************************/
    /* This class is for 'AlertBox' components                                                   */
    /*********************************************************************************************/
    public partial class AlertBox : ComponentBase
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

        /*---------------------------------------------------------------------------------------*/
        /* Private, local variables and objects                                                  */
        /*---------------------------------------------------------------------------------------*/
        private bool IsVisible = false;                                                  // Hide/Show the alertbox
        private string Title;                                                            // This is written at the top of the box
        private string Text;                                                             // This is showed in the body of box
        private AlertBoxType Type;                                                       // Type of alert: info, warning, error, question

        public delegate void ConfirmMethod();
        private ConfirmMethod OnConfirm = null;                                          // What should be executed after confirmation
        private ConfirmMethod OnCancel = null;                                           // What should be executed if question answer is no

        /*=======================================================================================*/
        /* Methods                                                                               */
        /*=======================================================================================*/
        /*---------------------------------------------------------------------------------------*/
        /* Function name: SetAlert                                                               */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method updates variables in box and display it. Alert cna be invoked by this.    */
        /*---------------------------------------------------------------------------------------*/
        public void SetAlert(string title, string text, AlertBoxType type)
        {
            Title = title;
            Text = text;
            Type = type;
            IsVisible = true;
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: SetAlert                                                               */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method updates variables in box and display it. Alert cna be invoked by this.    */
        /*---------------------------------------------------------------------------------------*/
        public void SetAlert(string title, string text, AlertBoxType type, ConfirmMethod onConfirm)
        {
            OnConfirm = onConfirm;
            Title = title;
            Text = text;
            Type = type;
            IsVisible = true;
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: SetAlert                                                               */
        /*                                                                                       */
        /* Description:                                                                          */
        /* This method updates variables in box and display it. Alert cna be invoked by this.    */
        /*---------------------------------------------------------------------------------------*/
        public void SetAlert(string title, string text, AlertBoxType type, ConfirmMethod onConfirm, ConfirmMethod onCancel)
        {
            OnConfirm = onConfirm;
            OnCancel = onCancel;
            Title = title;
            Text = text;
            Type = type;
            IsVisible = true;
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: HideQuestionNo                                                         */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Executed if type was 'Question' and user cancelled it.                                */
        /*---------------------------------------------------------------------------------------*/
        public void HideQuestionNo()
        {
            if (OnCancel != null)
                OnCancel.Invoke();

            OnConfirm = null;
            OnCancel = null;
            IsVisible = false;
            StateHasChanged();
        }

        /*---------------------------------------------------------------------------------------*/
        /* Function name: Hide                                                                   */
        /*                                                                                       */
        /* Description:                                                                          */
        /* Executed after confirmation                                                           */
        /*---------------------------------------------------------------------------------------*/
        public void Hide()
        {
            IsVisible = false;
            StateHasChanged();

            if (OnConfirm != null)
                OnConfirm.Invoke();

            OnConfirm = null;
            OnCancel = null;
        }

        /*=======================================================================================*/
        /* Enums                                                                                 */
        /*=======================================================================================*/
        public enum AlertBoxType
        {
            Info,
            Warning,
            Error,
            Question
        }
    }
}
