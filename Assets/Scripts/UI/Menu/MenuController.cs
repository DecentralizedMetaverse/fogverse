using Teo.AutoReference;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [GetInChildren, Name("MainMenu"), SerializeField] private MenuPresenter presenter;
    private void Start()
    {
        InputF.action.Common.Menu.performed += _ => ToggleMenu();
    }

    private void ToggleMenu()
    {
        if (presenter.IsShowing)
        {
            presenter.Close();
        }
        else
        {
            presenter.Show();
        }
    }
}
