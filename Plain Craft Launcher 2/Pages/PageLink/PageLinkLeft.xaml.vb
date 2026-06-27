Public Class PageLinkLeft

    Public PageID As FormMain.PageSubType = FormMain.PageSubType.LinkLobby

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub PageCheck(sender As MyListItem, e As EventArgs) Handles ItemLobby.Check, ItemFriend.Check, ItemServer.Check
        If sender.Tag IsNot Nothing Then PageChange(Val(sender.Tag))
    End Sub

    Public Function PageGet(Optional ID As FormMain.PageSubType = -1) As MyPageRight
        If ID = -1 Then ID = PageID
        If FrmLinkMain Is Nothing Then FrmLinkMain = New PageLinkMain
        FrmLinkMain.SetMaintenanceTitle(PageTitleGet(ID))
        Return FrmLinkMain
    End Function

    Public Sub PageChange(ID As FormMain.PageSubType)
        If PageID = ID Then Return
        AniControlEnabled += 1
        Try
            PageChangeRun(PageGet(ID))
            PageID = ID
        Catch ex As Exception
            Log(ex, "切换分页面失败（ID " & ID & "）", LogLevel.Feedback)
        Finally
            AniControlEnabled -= 1
        End Try
    End Sub

    Private Shared Function PageTitleGet(ID As FormMain.PageSubType) As String
        Select Case ID
            Case FormMain.PageSubType.LinkLobby
                Return "联机大厅"
            Case FormMain.PageSubType.LinkFriend
                Return "好友联机"
            Case FormMain.PageSubType.LinkServer
                Return "服务器"
            Case Else
                Return "联机"
        End Select
    End Function

    Private Shared Sub PageChangeRun(Target As MyPageRight)
        AniStop("FrmMain PageChangeRight")
        If Target.Parent IsNot Nothing Then Target.SetValue(ContentPresenter.ContentProperty, Nothing)
        FrmMain.PageRight = Target
        CType(FrmMain.PanMainRight.Child, MyPageRight).PageOnExit()
        AniStart({
            AaCode(
            Sub()
                CType(FrmMain.PanMainRight.Child, MyPageRight).PageOnForceExit()
                FrmMain.PanMainRight.Child = FrmMain.PageRight
                FrmMain.PageRight.Opacity = 0
            End Sub, 130),
            AaCode(
            Sub()
                FrmMain.PageRight.Opacity = 1
                FrmMain.PageRight.PageOnEnter()
            End Sub, 30, True)
        }, "PageLeft PageChange")
    End Sub

End Class
