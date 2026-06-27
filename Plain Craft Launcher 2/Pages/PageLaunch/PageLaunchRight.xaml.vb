Imports System.Windows.Threading
Imports System.IO

Public Class PageLaunchRight
    Implements IRefreshable, IDispatcherUnhandledException

    Private Sub Init() Handles Me.Loaded
        PanBack.ScrollToHome()
        PanLog.Visibility = If(ModeDebug, Visibility.Visible, Visibility.Collapsed)
        RefreshQuickPlayWorlds()
    End Sub

    Private Sub RefreshQuickPlayWorlds() Handles Me.PageEnter
        If Not IsLoaded Then Return
        RunInThread(
        Sub()
            Dim Instance As McInstance = McInstanceSelected
            Dim Supported As Boolean = IsQuickPlaySupported(Instance)
            Dim SavePath As String = GetSavesPath(Instance)
            Dim Worlds As New List(Of String)
            If SavePath <> "" AndAlso Directory.Exists(SavePath) Then
                For Each DirPath In Directory.GetDirectories(SavePath)
                    Try
                        Worlds.Add(System.IO.Path.GetFileName(DirPath))
                    Catch
                    End Try
                Next
                Worlds = Worlds.Distinct().OrderBy(Function(s) s, StringComparer.OrdinalIgnoreCase).ToList()
            End If
            RunInUi(Sub() LoadQuickPlayWorldList(Worlds, Supported))
        End Sub)
    End Sub

    Private Shared Function GetSavesPath(Instance As McInstance) As String
        If Instance Is Nothing Then Return ""
        Dim Root As String = Instance.PathIndie
        If String.IsNullOrEmpty(Root) Then Return ""
        Dim A As String = Root & "saves\"
        If Directory.Exists(A) Then Return A
        Dim B As String = Root & "save\"
        If Directory.Exists(B) Then Return B
        Return A
    End Function

    Private Shared Function IsQuickPlaySupported(Instance As McInstance) As Boolean
        If Instance Is Nothing Then Return False
        Dim Name As String = If(Instance.Version?.VanillaName, "")
        If Name = "" OrElse Name = "Unknown" Then Return False
        If Name.StartsWithF("1.") Then Return Instance.Version.Vanilla.Major >= 20
        If RegexCheck(Name, "^[2-9][0-9]*\.") Then Return True
        Return False
    End Function

    Private Sub LoadQuickPlayWorldList(Worlds As List(Of String), Supported As Boolean)
        PanQuickPlayList.Children.Clear()
        If Worlds Is Nothing OrElse Worlds.Count = 0 Then
            LabQuickPlayEmpty.Text = "未找到世界"
            LabQuickPlayEmpty.Visibility = Visibility.Visible
            Return
        End If
        LabQuickPlayEmpty.Visibility = Visibility.Collapsed

        For Each World In Worlds
            Dim Row As New Border With {
                .CornerRadius = New CornerRadius(6),
                .Margin = New Thickness(0, 0, 0, 8),
                .Background = ColorSemiTransparent,
                .SnapsToDevicePixels = True
            }
            Row.SetResourceReference(Border.BackgroundProperty, "ColorBrushSemiTransparent")

            Dim Grid As New Grid With {.Margin = New Thickness(12, 8, 12, 8)}
            Grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            Grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            Grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            Grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            Dim NameBlock As New TextBlock With {
                .Text = World,
                .VerticalAlignment = VerticalAlignment.Center,
                .TextTrimming = TextTrimming.CharacterEllipsis
            }
            NameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrush1")
            Grid.Children.Add(NameBlock)

            Dim BtnStart As New MyIconButton With {
                .ImageSource = New MyBitmap("pack://application:,,,/images/Icons/WorldPlayStart.png"),
                .LogoScale = 1.0,
                .Margin = New Thickness(3, 0, 0, 0),
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Height = 32,
                .Width = 32,
                .Tag = World,
                .ToolTip = "启动世界"
            }
            If Not Supported Then
                BtnStart.IsEnabled = False
                BtnStart.ToolTip = "仅支持 1.20 及以上的版本"
            End If
            AddHandler BtnStart.Click, AddressOf QuickPlayStart_Click
            Grid.SetColumn(BtnStart, 1)
            Grid.Children.Add(BtnStart)

            Dim BtnBackup As New MyIconButton With {
                .ImageSource = New MyBitmap("pack://application:,,,/images/Icons/WorldPlayBackup.png"),
                .LogoScale = 1.0,
                .Margin = New Thickness(3, 0, 0, 0),
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Height = 32,
                .Width = 32,
                .Tag = World,
                .ToolTip = "备份世界"
            }
            AddHandler BtnBackup.Click, AddressOf QuickPlayBackup_Click
            Grid.SetColumn(BtnBackup, 2)
            Grid.Children.Add(BtnBackup)

            Dim BtnDelete As New MyIconButton With {
                .ImageSource = New MyBitmap("pack://application:,,,/images/Icons/WorldPlayDelete.png"),
                .LogoScale = 1.0,
                .Margin = New Thickness(3, 0, 0, 0),
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Height = 32,
                .Width = 32,
                .Tag = World,
                .ToolTip = "删除世界"
            }
            AddHandler BtnDelete.Click, AddressOf QuickPlayDelete_Click
            Grid.SetColumn(BtnDelete, 3)
            Grid.Children.Add(BtnDelete)

            If Not Supported Then
                Row.Tag = True
                AddHandler Row.MouseLeftButtonUp,
                Sub(sender, e)
                    Hint("点击仅支持 1.20 及以上的版本。", HintType.Red)
                End Sub
            End If

            Row.Child = Grid
            PanQuickPlayList.Children.Add(Row)
        Next
    End Sub

    Private Sub QuickPlayStart_Click(sender As Object, e As EventArgs)
        Dim World As String = TryCast(CType(sender, FrameworkElement).Tag, String)
        If String.IsNullOrEmpty(World) Then Return
        If Not IsQuickPlaySupported(McInstanceSelected) Then
            Hint("点击仅支持 1.20 及以上的版本。", HintType.Red)
            Return
        End If
        World = World.Replace("""", "")
        McLaunchStart(New McLaunchOptions With {.QuickPlaySingleplayer = World})
    End Sub

    Private Sub QuickPlayBackup_Click(sender As Object, e As EventArgs)
        Dim World As String = TryCast(CType(sender, FrameworkElement).Tag, String)
        If String.IsNullOrEmpty(World) Then Return
        Dim Instance As McInstance = McInstanceSelected
        If Instance Is Nothing Then Return
        Dim SavePath As String = GetSavesPath(Instance)
        If String.IsNullOrEmpty(SavePath) OrElse Not Directory.Exists(SavePath) Then Return
        Dim WorldPath As String = SavePath & World
        If Not Directory.Exists(WorldPath) Then Return
        Try
            Dim BackupName As String = $"{World} - 备份 {DateTime.Now:yyyy-MM-dd HH-mm-ss}"
            Dim BackupPath As String = SavePath & BackupName
            CopyDirectory(WorldPath, BackupPath)
            Hint($"已成功备份世界：{World}", HintType.Green)
            RefreshQuickPlayWorlds()
        Catch ex As Exception
            Log(ex, "备份世界失败", LogLevel.Msgbox)
        End Try
    End Sub

    Private Sub QuickPlayDelete_Click(sender As Object, e As EventArgs)
        Dim World As String = TryCast(CType(sender, FrameworkElement).Tag, String)
        If String.IsNullOrEmpty(World) Then Return
        Dim Instance As McInstance = McInstanceSelected
        If Instance Is Nothing Then Return
        Dim SavePath As String = GetSavesPath(Instance)
        If String.IsNullOrEmpty(SavePath) OrElse Not Directory.Exists(SavePath) Then Return
        Dim WorldPath As String = SavePath & World
        If Not Directory.Exists(WorldPath) Then Return
        If MyMsgBox($"确定要删除世界「{World}」吗？此操作无法撤销！", "确认删除", "确定", "取消") = 2 Then Return
        Try
            Directory.Delete(WorldPath, True)
            Hint($"已删除世界：{World}", HintType.Green)
            RefreshQuickPlayWorlds()
        Catch ex As Exception
            Log(ex, "删除世界失败", LogLevel.Msgbox)
        End Try
    End Sub

#Region "主页"

    ''' <summary>
    ''' 刷新主页。
    ''' </summary>
    Private Sub Refresh() Handles Me.Loaded
        RunInThread(
        Sub()
            Try
                SyncLock RefreshLock
                    RefreshReal()
                End SyncLock
            Catch ex As Exception
                Log(ex, "加载 PCL 主页自定义信息失败", If(ModeDebug, LogLevel.Msgbox, LogLevel.Hint))
            End Try
        End Sub)
    End Sub
    Private Sub RefreshReal()
        Dim Content As String = Nothing, Url As String = Nothing
        Select Case Setup.Get("UiCustomType")
            Case 1
                '加载本地文件
                Log("[Page] 主页自定义数据来源：本地文件")
                Content = ReadFile(Path & "PCL\Custom.xaml") 'ReadFile 会进行存在检测
            Case 2
                '联网下载
                Url = Setup.Get("UiCustomNet")
            Case 3
                '预设
                Select Case Setup.Get("UiCustomPreset")
                    Case 0
                        Log("[Page] 主页预设：你知道吗")
                        Content = "
                            <local:MyCard Title=""你知道吗？"" Margin=""0,0,0,15"">
                                <TextBlock Margin=""25,38,23,15"" FontSize=""13.5"" IsHitTestVisible=""False"" Text=""{hint}"" TextWrapping=""Wrap"" Foreground=""{DynamicResource ColorBrush1}"" />
                                <local:MyIconButton Height=""22"" Width=""22"" Margin=""9"" VerticalAlignment=""Top"" HorizontalAlignment=""Right"" 
                                    EventType=""刷新主页"" EventData=""/""
                                    Logo=""M875.52 148.48C783.36 56.32 655.36 0 512 0 291.84 0 107.52 138.24 30.72 332.8l122.88 46.08C204.8 230.4 348.16 128 512 128c107.52 0 199.68 40.96 271.36 112.64L640 384h384V0L875.52 148.48zM512 896c-107.52 0-199.68-40.96-271.36-112.64L384 640H0v384l148.48-148.48C240.64 967.68 368.64 1024 512 1024c220.16 0 404.48-138.24 481.28-332.8L870.4 645.12C819.2 793.6 675.84 896 512 896z"" />
                            </local:MyCard>"
                    Case 1
                        Log("[Page] 主页预设：回声洞")
                        Content = "
                            <local:MyCard Title=""回声洞"" Margin=""0,0,0,15"">
                                <TextBlock Margin=""25,38,23,15"" FontSize=""13.5"" IsHitTestVisible=""False"" Text=""{cave}"" TextWrapping=""Wrap"" Foreground=""{DynamicResource ColorBrush1}"" />
                                <local:MyIconButton Height=""22"" Width=""22"" Margin=""9"" VerticalAlignment=""Top"" HorizontalAlignment=""Right"" 
                                    EventType=""刷新主页"" EventData=""/""
                                    Logo=""M875.52 148.48C783.36 56.32 655.36 0 512 0 291.84 0 107.52 138.24 30.72 332.8l122.88 46.08C204.8 230.4 348.16 128 512 128c107.52 0 199.68 40.96 271.36 112.64L640 384h384V0L875.52 148.48zM512 896c-107.52 0-199.68-40.96-271.36-112.64L384 640H0v384l148.48-148.48C240.64 967.68 368.64 1024 512 1024c220.16 0 404.48-138.24 481.28-332.8L870.4 645.12C819.2 793.6 675.84 896 512 896z"" />
                            </local:MyCard>"
                    Case 2
                        Log("[Page] 主页预设：Minecraft 新闻")
                        Url = "https://mcnews.meloong.com"
                    Case 3
                        Log("[Page] 主页预设：简单主页")
                        Url = "https://pclhomeplazaoss.lingyunawa.top:26994/d/Homepages/MFn233/Custom.xaml"
                    Case 4
                        Log("[Page] 主页预设：每日整合包推荐")
                        Url = "https://pclsub.sodamc.com/"
                    Case 5
                        Log("[Page] 主页预设：Minecraft 皮肤推荐")
                        Url = "https://forgepixel.com/pcl_sub_file"
                    Case 6
                        Log("[Page] 主页预设：OpenBMCLAPI 仪表盘 Lite")
                        Url = "https://pcl-bmcl.milu.ink/"
                    Case 7
                        Log("[Page] 主页预设：主页市场")
                        Url = "https://pclhomeplazaoss.lingyunawa.top:26994/p/Homepages/Homepage.Market/Custom.xaml"
                    Case 8
                        Log("[Page] 主页预设：更新日志")
                        Url = "https://pclhomeplazaoss.lingyunawa.top:26994/d/Homepages/Joker2184/UpdateHomepage.xaml"
                    Case 9
                        Log("[Page] 主页预设：PCL 新功能说明书")
                        Url = "https://raw.gitcode.com/WForst-Breeze/WhatsNewPCL/raw/main/Custom.xaml"
                    Case 10
                        Log("[Page] 主页预设：OpenMCIM Dashboard")
                        Url = "https://files.mcimirror.top/PCL"
                    Case 11
                        Log("[Page] 主页预设：杂志主页")
                        Url = "http://118.195.192.193:26995/d/magazine-homepage-pcl/Custom.xaml"
                    Case 12
                        Log("[Page] 主页预设：PCL GitHub 仪表盘")
                        Url = "https://ddf.pcl-community.org/Custom.xaml"
                    Case 13
                        Log("[Page] 主页预设：PCL 更新摘要")
                        Url = "https://raw.gitcode.com/ENC_Euphony/PCL-AI-Summary-HomePage/raw/master/Custom.xaml"
                    Case 14
                        Log("[Page] 主页预设：今日新闻热点")
                        Url = "https://pcl.wyc-w.top/index.xaml"
                    Case 15
                        Log("[Page] 主页预设：Minecraft 芝士站")
                        Url = "https://www.xxag.top/mkss"
                End Select
        End Select
        '联网下载
        If Not String.IsNullOrWhiteSpace(Url) Then
            If Url = Setup.Get("CacheSavedPageUrl") AndAlso File.Exists(PathTemp & "Cache\Custom.xaml") Then
                '缓存可用
                Log("[Page] 主页自定义数据来源：联网缓存文件")
                Content = ReadFile(PathTemp & "Cache\Custom.xaml")
                '后台更新缓存
                OnlineLoader.Start(New Tuple(Of String, Boolean)(Url, False))
            Else
                '缓存不可用
                Log("[Page] 主页自定义数据来源：联网全新下载")
                Hint("正在加载主页……")
                RunInUiWait(Sub() LoadContent(Nothing)) '在加载结束前清空页面
                Setup.Set("CacheSavedPageVersion", "")
                OnlineLoader.Start(New Tuple(Of String, Boolean)(Url, True)) '下载完成后将会再次触发更新
                Return
            End If
        End If
        '同步到 UI
        RunInUi(Sub() LoadContent(Content))
    End Sub
    Private RefreshLock As New Object

    '联网获取主页文件
    Private OnlineLoader As New LoaderTask(Of Tuple(Of String, Boolean), Integer)("下载主页", AddressOf OnlineLoaderSub) With {.ReloadTimeout = 10 * 60 * 1000}
    Private Sub OnlineLoaderSub(Task As LoaderTask(Of Tuple(Of String, Boolean), Integer))
        Dim Address As String = Task.Input.Item1 '#3721 中连续触发两次导致内容变化
        Dim ShouldRefresh As Boolean = Task.Input.Item2
        Try
            '替换自定义变量与设置
            Address = ArgumentReplace(Address, AddressOf WebUtility.HtmlEncode)
            '获取版本校验地址
            Dim VersionAddress As String
            If Address.Contains(".xaml") Then
                VersionAddress = Address.Replace(".xaml", ".xaml.ini")
            Else
                VersionAddress = Address.BeforeFirst("?")
                If Not VersionAddress.EndsWith("/") Then VersionAddress += "/"
                VersionAddress += "version"
                If Address.Contains("?") Then VersionAddress += "?" & Address.AfterFirst("?")
            End If
            '校验版本
            Dim Version As String = ""
            Try
                Version = NetRequestByClientRetry(VersionAddress)
                If Version.Length > 1000 Then Throw New Exception($"获取的主页版本过长（{Version.Length} 字符）")
                Dim CurrentVersion As String = Setup.Get("CacheSavedPageVersion")
                If Version <> "" AndAlso CurrentVersion <> "" AndAlso Version = CurrentVersion Then
                    Log($"[Page] 当前缓存的主页已为最新，当前版本：{Version}，检查源：{VersionAddress}")
                    Return
                End If
                Log($"[Page] 需要下载联网主页，当前版本：{Version}，检查源：{VersionAddress}")
            Catch exx As Exception
                Log(exx, $"联网获取主页版本失败", LogLevel.Developer)
                Log($"[Page] 无法检查联网主页版本，将直接下载，检查源：{VersionAddress}")
            End Try
            '实际下载
            Dim FileContent As String = NetRequestByClientRetry(Address)
            Log($"[Page] 已联网下载主页，内容长度：{FileContent.Length}，来源：{Address}")
            Setup.Set("CacheSavedPageUrl", Address)
            Setup.Set("CacheSavedPageVersion", Version)
            WriteFile(PathTemp & "Cache\Custom.xaml", FileContent)
            '若内容变更则要求刷新
            If LoadedContentHash <> FileContent.GetHashCode() AndAlso ShouldRefresh Then Refresh()
        Catch ex As Exception
            Log(ex, $"下载主页失败（{Address}）", If(ModeDebug, LogLevel.Msgbox, LogLevel.Hint))
        End Try
    End Sub

    ''' <summary>
    ''' 立即强制刷新主页。
    ''' 必须在 UI 线程调用。
    ''' </summary>
    Public Sub ForceRefresh() Implements IRefreshable.Refresh
        Log("[Page] 要求强制刷新主页")
        ClearCache()
        '实际的刷新
        If FrmMain.PageCurrent.Page = FormMain.PageType.Launch Then
            PanBack.ScrollToHome()
            Refresh()
        Else
            FrmMain.PageChange(FormMain.PageType.Launch)
        End If
    End Sub

    ''' <summary>
    ''' 清空主页缓存信息。
    ''' </summary>
    Private Sub ClearCache()
        LoadedContentHash = -1
        OnlineLoader.Input = New Tuple(Of String, Boolean)("", True)
        Setup.Set("CacheSavedPageUrl", "")
        Setup.Set("CacheSavedPageVersion", "")
        Log("[Page] 已清空主页缓存")
    End Sub

    ''' <summary>
    ''' 从文本内容中加载主页。
    ''' 必须在 UI 线程调用。
    ''' </summary>
    Private Sub LoadContent(Content As String)
        Try
            SyncLock LoadContentLock
                '如果加载目标内容一致则不加载
                Dim Hash = If(Content, "").GetHashCode()
                If Hash = LoadedContentHash Then Return
                LoadedContentHash = Hash
                '实际加载内容
                PanCustom.Children.Clear()
                If String.IsNullOrWhiteSpace(Content) Then
                    Log($"[Page] 实例化：清空主页 UI，来源为空")
                    Return
                End If
                Dim LoadStartTime As Date = Date.Now
                '修改时应同时修改 PageOtherHelpDetail.Init
                Content = ArgumentReplace(Content, AddressOf EscapeXML)
                Do While Content.Contains("xmlns")
                    Content = Content.RegexReplace("xmlns[^""']*(""|')[^""']*(""|')", "").Replace("xmlns", "") '禁止声明命名空间
                Loop
                Content = "<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:sys=""clr-namespace:System;assembly=mscorlib"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:local=""clr-namespace:PCL;assembly=Plain Craft Launcher 2 Network Edition"">" & Content & "</StackPanel>"
                Log($"[Page] 实例化：加载主页 UI 开始，最终内容长度：{Content.Count}")
                PanCustom.Children.Add(GetObjectFromXML(Content))
                '加载计时
                Dim LoadCostTime = (Date.Now - LoadStartTime).Milliseconds
                Log($"[Page] 实例化：加载主页 UI 完成，耗时 {LoadCostTime}ms")
                If LoadCostTime > 3000 Then Hint($"主页加载过于缓慢（花费了 {Math.Round(LoadCostTime / 1000, 1)} 秒），请向主页作者反馈此问题，或暂时停止使用该主页")
            End SyncLock
        Catch ex As Exception
            Log(ex, "加载失败的主页内容：" & vbCrLf & Content)
            OnLoadContentFailed(ex)
        End Try
    End Sub
    ''' <summary>
    ''' 加载主页失败时调用。
    ''' </summary>
    Private Sub OnLoadContentFailed(ex As Exception)
        If ModeDebug OrElse Setup.Get("UiCustomType") = 1 Then
            Log(ex, "加载主页失败")
            If MyMsgBox(If(TypeOf ex Is UnauthorizedAccessException, ex.Message, $"主页内容编写有误，请根据下列错误信息进行检查：{vbCrLf}{ex.GetBrief}"),
                        "加载主页失败", "重试", "取消") = 1 Then ForceRefresh()
        Else
            Log(ex, "加载主页失败", LogLevel.Hint)
        End If
    End Sub
    ''' <summary>
    ''' 捕获主页在 Measure 和 Arrange 阶段抛出的异常。
    ''' </summary>
    Private Sub DispatcherUnhandledException(sender As Object, e As DispatcherUnhandledExceptionEventArgs) Implements IDispatcherUnhandledException.DispatcherUnhandledException
        If TypeOf e.Exception IsNot Markup.XamlParseException Then Return
        e.Handled = True
        LoadContent(Nothing)
        OnLoadContentFailed(e.Exception)
    End Sub

    Private LoadedContentHash As Integer = -1
    Private LoadContentLock As New Object

#End Region

End Class
