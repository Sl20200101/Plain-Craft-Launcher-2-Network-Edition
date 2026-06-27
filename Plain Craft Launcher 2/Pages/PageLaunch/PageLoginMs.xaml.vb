Imports System.Net
Imports System.Threading.Tasks
Imports System.Linq
Imports Newtonsoft.Json.Linq

Public Class PageLoginMs

    ''' <summary>
    ''' 刷新页面显示的所有信息。
    ''' </summary>
    Public Sub Reload(KeepInput As Boolean)
        Dim IndexBefore = ComboAccounts.SelectedIndex
        '刷新下拉框列表
        ComboAccounts.Items.Clear()
        ComboAccounts.Items.Add(New MyComboBoxItem With {.Content = "添加新账号"})
        Try
            Dim MsJson As JObject = GetJson(Setup.Get("LoginMsJson"))
            For Each Account In MsJson
                Dim Item As MyListItem = CType(FindResource("ComboBoxItemTemplateWithDelete"), DataTemplate).LoadContent()
                Item.Tag = Account.Value.ToString
                Item.Title = Account.Key
                CType(Item.Buttons(0), MyIconButton).Tag = Account.Key
                ComboAccounts.Items.Add(Item)
            Next
        Catch ex As Exception
            Log(ex, $"微软登录信息出错，登录信息已被重置（{Setup.Get("LoginMsJson")}）", LogLevel.Hint)
            Setup.Set("LoginMsJson", "{}")
        End Try
        '如果不保留输入，刷新列表后自动选择第一项
        ComboAccounts.SelectedIndex = If(KeepInput, Math.Max(0, IndexBefore), 0)
    End Sub
    Private Sub ComboAccounts_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboAccounts.SelectionChanged
        If AniControlEnabled <> 0 OrElse ComboAccounts.SelectedItem Is Nothing OrElse ComboAccounts.ContentPresenter Is Nothing Then Return
        If TypeOf ComboAccounts.SelectedItem Is MyListItem Then
            ComboAccounts.ContentPresenter.Content = CType(ComboAccounts.SelectedItem, MyListItem).Title
        ElseIf TypeOf ComboAccounts.SelectedItem Is MyComboBoxItem Then
            ComboAccounts.ContentPresenter.Content = CType(ComboAccounts.SelectedItem, MyComboBoxItem).Content
        End If
    End Sub

    ''' <summary>
    ''' 获取当前页面的登录信息。
    ''' </summary>
    Public Shared Function GetLoginData() As McLoginMs
        If FrmLoginMs Is Nothing Then Return New McLoginMs With {.OAuthRefreshToken = Setup.Get("CacheMsV2OAuthRefresh"), .UserName = Setup.Get("CacheMsV2Name")}
        Dim Result As McLoginMs = Nothing
        RunInUiWait(
        Sub()
            If FrmLoginMs.ComboAccounts.SelectedIndex = 0 Then
                Result = New McLoginMs
            Else
                Dim Item As MyListItem = FrmLoginMs.ComboAccounts.SelectedItem
                Result = New McLoginMs With {.OAuthRefreshToken = Item.Tag, .UserName = Item.Title}
            End If
        End Sub)
        Return Result
    End Function
    ''' <summary>
    ''' 当前页面的登录信息是否有效。
    ''' </summary>
    Public Shared Function IsVaild(LoginData As McLoginMs) As String
        If LoginData.OAuthRefreshToken = "" Then
            Return "请在登录账号后再启动游戏！"
        Else
            Return ""
        End If
    End Function
    Public Function IsVaild() As String
        Return IsVaild(GetLoginData())
    End Function

    Private Async Sub BtnLogin_Click(sender As Object, e As EventArgs) Handles BtnLogin.Click
        ' 禁用按钮以防重复点击
        ComboAccounts.IsEnabled = False
        BtnLogin.IsEnabled = False
        BtnLogin.Text = "正在登录"
        TextCode.Visibility = Visibility.Collapsed ' 隐藏之前的授权码显示

        Try
            ' 1. 打开浏览器，请求授权码
            ' client_id, redirect_uri, scope 等参数按要求配置
            Dim ClientId = OAuthClientId
            Dim RedirectUri = "https://login.live.com/oauth20_desktop.srf"
            Dim Scope = "XboxLive.signin offline_access"
            Dim AuthUrl = $"https://login.live.com/oauth20_authorize.srf?client_id={ClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(RedirectUri)}&scope={Uri.EscapeDataString(Scope)}"

            ' 使用项目内置方法打开网页
            OpenWebsite(AuthUrl)

            ' 2. 等待用户输入回调网址
            Dim RedirectedUrl As String = MyMsgBoxInput("请在浏览器登录并授权后，将地址栏中显示的空白页面的网址完整复制到下方。", "输入重定向网址")
            If String.IsNullOrWhiteSpace(RedirectedUrl) Then Throw New OperationCanceledException("用户取消了登录。")

            ' 3. 从 URL 中解析 code
            Dim Code As String = RedirectedUrl.Trim()
            If Code.Contains("code=") Then
                Code = Code.Split("code=")(1).Split("&")(0)
            End If
            ' 解码以防万一（处理 URL 编码）
            Code = System.Net.WebUtility.UrlDecode(Code)

            ' 4. 当捕获到 code 时进行处理
            If Not String.IsNullOrEmpty(Code) Then
                ' 5. 使用 code 换取 Access Token
                Log("[OAuth] 正在使用 code 换取 Access Token...")
                Dim TokenUrl = "https://login.live.com/oauth20_token.srf"
                Dim TokenParams = $"client_id={ClientId}&grant_type=authorization_code&code={Uri.EscapeDataString(Code)}&redirect_uri={Uri.EscapeDataString(RedirectUri)}&scope={Uri.EscapeDataString(Scope)}"

                ' 由于 NetRequestByClientMultiple 内部会检查是否在 UI 线程执行，需在 Task.Run 中执行
                Dim TokenResponse As String = Await Task.Run(Function()
                                                                Return NetRequestByClientMultiple(TokenUrl, HttpMethod.Post,
                                                                                                 Content:=TokenParams,
                                                                                                 ContentType:="application/x-www-form-urlencoded",
                                                                                                 RequireJson:=True)
                                                            End Function)

                Dim TokenJson As JObject = GetJson(TokenResponse)
                Dim AccessToken = TokenJson("access_token").ToString()

                ' 6. 登录 Xbox Live 获取 XBL Token 和 UHS
                Log("[OAuth] 正在登录 Xbox Live...")
                Dim XblUrl = "https://user.auth.xboxlive.com/user/authenticate"
                Dim XblRequest = New JObject(
                    New JProperty("Properties", New JObject(
                        New JProperty("AuthMethod", "RPS"),
                        New JProperty("SiteName", "user.auth.xboxlive.com"),
                        New JProperty("RpsTicket", "d=" & AccessToken)
                    )),
                    New JProperty("RelyingParty", "http://auth.xboxlive.com"),
                    New JProperty("TokenType", "JWT")
                ).ToString(Newtonsoft.Json.Formatting.None)

                Dim XblResponse As String = Await Task.Run(Function()
                                                              Return NetRequestByClientMultiple(XblUrl, HttpMethod.Post,
                                                                                               Content:=XblRequest,
                                                                                               ContentType:="application/json",
                                                                                               Accept:="application/json",
                                                                                               RequireJson:=True)
                                                          End Function)

                Dim XblJson As JObject = GetJson(XblResponse)
                Dim XblToken = XblJson("Token").ToString()
                Dim Uhs = XblJson("DisplayClaims")("xui")(0)("uhs").ToString()

                ' 7. 使用 XBL Token 换取 XSTS Token
                Log("[OAuth] 正在换取 XSTS Token...")
                Dim XstsUrl = "https://xsts.auth.xboxlive.com/xsts/authorize"
                Dim XstsRequest = New JObject(
                    New JProperty("Properties", New JObject(
                        New JProperty("SandboxId", "RETAIL"),
                        New JProperty("UserTokens", New JArray(XblToken))
                    )),
                    New JProperty("RelyingParty", "rp://api.minecraftservices.com/"),
                    New JProperty("TokenType", "JWT")
                ).ToString(Newtonsoft.Json.Formatting.None)

                Dim XstsResponse As String = Await Task.Run(Function()
                                                               Return NetRequestByClientMultiple(XstsUrl, HttpMethod.Post,
                                                                                                Content:=XstsRequest,
                                                                                                ContentType:="application/json",
                                                                                                Accept:="application/json",
                                                                                                RequireJson:=True)
                                                           End Function)

                Dim XstsJson As JObject = GetJson(XstsResponse)
                Dim XstsToken = XstsJson("Token").ToString()

                ' 8. 使用 XSTS Token 登录 Minecraft 获取最终的 Access Token
                Log("[OAuth] 正在登录 Minecraft...")
                Dim McLoginUrl = "https://api.minecraftservices.com/authentication/login_with_xbox"
                Dim McLoginRequest = New JObject(
                    New JProperty("identityToken", $"XBL3.0 x={Uhs};{XstsToken}")
                ).ToString(Newtonsoft.Json.Formatting.None)

                Dim McLoginResponse As String = Await Task.Run(Function()
                                                                  Return NetRequestByClientMultiple(McLoginUrl, HttpMethod.Post,
                                                                                                   Content:=McLoginRequest,
                                                                                                   ContentType:="application/json",
                                                                                                   Accept:="application/json",
                                                                                                   RequireJson:=True)
                                                              End Function)

                Dim McLoginJson As JObject = GetJson(McLoginResponse)
                Dim SuccessToken = McLoginJson("access_token").ToString()

                ' 9. 验证正版所有权
                Log("[OAuth] 正在验证正版所有权...")
                Dim EntitlementUrl = "https://api.minecraftservices.com/entitlements/mcstore"
                Dim EntitlementResponse As String = Await Task.Run(Function()
                                                                      Return NetRequestByClientMultiple(EntitlementUrl, HttpMethod.Get,
                                                                                                       Headers:={{"Authorization", "Bearer " & SuccessToken}},
                                                                                                       RequireJson:=True)
                                                                  End Function)
                Dim EntitlementJson As JObject = GetJson(EntitlementResponse)
                If Not (EntitlementJson.ContainsKey("items") AndAlso EntitlementJson("items").Any) Then
                    Hint("你尚未购买正版 Minecraft，或者 Xbox Game Pass 已到期。", HintType.Red)
                    If MyMsgBox("你尚未购买正版 Minecraft，或者 Xbox Game Pass 已到期。", "登录失败", "购买 Minecraft", "取消") = 1 Then
                        OpenWebsite("https://www.xbox.com/zh-cn/games/store/minecraft-java-bedrock-edition-for-pc/9nxp44l49shj")
                    End If
                    Return
                End If

                ' 10. 获取玩家档案
                Log("[OAuth] 正在获取玩家档案...")
                Dim ProfileUrl = "https://api.minecraftservices.com/minecraft/profile"
                Dim ProfileResponse As String = Await Task.Run(Function()
                                                                  Return NetRequestByClientMultiple(ProfileUrl, HttpMethod.Get,
                                                                                                   Headers:={{"Authorization", "Bearer " & SuccessToken}},
                                                                                                   RequireJson:=True)
                                                              End Function)
                Dim ProfileJson As JObject = GetJson(ProfileResponse)
                Dim UserName = ProfileJson("name").ToString()
                Dim UUID = ProfileJson("id").ToString()

                ' 11. 保存登录信息并刷新 UI
                Log("[OAuth] 正在保存登录信息...")
                Setup.Set("CacheMsV2Access", SuccessToken)
                Setup.Set("CacheMsV2Name", UserName)
                Setup.Set("CacheMsV2Uuid", UUID)
                Setup.Set("CacheMsV2ProfileJson", ProfileResponse)
                ' 如果有 Refresh Token 也保存（当前流程是从 Code 获取，通常会有 refresh_token）
                If TokenJson.ContainsKey("refresh_token") Then
                    Setup.Set("CacheMsV2OAuthRefresh", TokenJson("refresh_token").ToString())
                End If

                ' 刷新左侧启动栏，切换到皮肤显示界面
                RunInUi(Sub() FrmLaunchLeft.RefreshPage(False, True))

                ' 在 UI 上显示 Success Token 和玩家名
                TextCode.Text = $"Success Token: {SuccessToken}{vbCrLf}User Name: {UserName}{vbCrLf}Microsoft Access Code: {Code}"
                TextCode.Visibility = Visibility.Visible
                Hint($"Minecraft 登录成功：{UserName}", HintType.Green)
                Log($"[OAuth] 已成功获取 Minecraft Success Token: {SuccessToken}")
            Else
                Hint("登录失败：未收到授权码。", HintType.Red)
            End If

        Catch ex As Exception
            ' 异常处理与日志记录
            Log(ex, "微软登录 OAuth 流程出错", LogLevel.Msgbox)
        Finally
            ' 恢复按钮状态
            ComboAccounts.IsEnabled = True
            BtnLogin.IsEnabled = True
            BtnLogin.Text = "登录"
        End Try
    End Sub

End Class
