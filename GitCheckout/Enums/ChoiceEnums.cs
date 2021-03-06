namespace GitCheckout.Enums
{
    internal enum ProtocolChoices
    {
        GitHub,
        SourceTree,
        Custom
    }

    internal enum MainMenuChoices
    {
        Protocols,
        Directories,
        Exit
    }

    internal enum ManageChoices
    {
        Add,
        Update,
        Remove,
        ReRegister,
        Deregister,
        List,
        SetDefault,
        ClearDefault,
        Return
    }
}