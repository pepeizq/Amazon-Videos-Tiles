Imports Microsoft.Toolkit.Uwp.UI.Controls
Imports Windows.Storage
Imports Windows.UI

Namespace Interfaz

    Module Videos

        Public anchoColumna As Integer = 350

        Public Sub Cargar()

            Dim recursos As New Resources.ResourceLoader

            Dim frame As Frame = Window.Current.Content
            Dim pagina As Page = frame.Content

            Dim botonBuscarVideos As Button = pagina.FindName("botonBuscarVideos")
            botonBuscarVideos.IsEnabled = False

            AddHandler botonBuscarVideos.Click, AddressOf BuscarVideosClick
            AddHandler botonBuscarVideos.PointerEntered, AddressOf EfectosHover.Entra_Boton_1_05
            AddHandler botonBuscarVideos.PointerExited, AddressOf EfectosHover.Sale_Boton_1_05

            Dim tbBuscadorVideos As TextBox = pagina.FindName("tbBuscadorVideos")
            tbBuscadorVideos.Tag = botonBuscarVideos
            AddHandler tbBuscadorVideos.TextChanged, AddressOf BuscadorVideosTextoCambia

        End Sub

        Private Sub BuscadorVideosTextoCambia(sender As Object, e As TextChangedEventArgs)

            Dim tb As TextBox = sender
            Dim boton As Button = tb.Tag

            If tb.Text.Trim.Length > 2 Then
                boton.IsEnabled = True
            Else
                boton.IsEnabled = False
            End If

        End Sub

        Private Async Sub BuscarVideosClick(sender As Object, e As RoutedEventArgs)

            Estado(False, False)

            Dim frame As Frame = Window.Current.Content
            Dim pagina As Page = frame.Content

            Dim pbBuscadorVideos As ProgressBar = pagina.FindName("pbBuscadorVideos")
            pbBuscadorVideos.Visibility = Visibility.Visible

            Dim tbBuscadorVideos As TextBox = pagina.FindName("tbBuscadorVideos")
            tbBuscadorVideos.IsEnabled = False

            Dim buscar As String = tbBuscadorVideos.Text.Trim

            Dim wvBuscador As WebView = pagina.FindName("wvBuscador")
            Await WebView.ClearTemporaryWebDataAsync()

            Dim config As ApplicationDataContainer = ApplicationData.Current.LocalSettings
            wvBuscador.Navigate(New Uri("https://www.primevideo.com/search/ref=atv_nb_sr?phrase=" + buscar))

            wvBuscador.Tag = buscar

            AddHandler wvBuscador.LoadCompleted, AddressOf WvBuscar

        End Sub

        Private Async Sub WvBuscar(sender As Object, e As NavigationEventArgs)

            Dim frame As Frame = Window.Current.Content
            Dim pagina As Page = frame.Content

            Dim botonBuscarVideos As Button = pagina.FindName("botonBuscarVideos")
            Dim pbBuscadorVideos As ProgressBar = pagina.FindName("pbBuscadorVideos")
            Dim tbBuscadorVideos As TextBox = pagina.FindName("tbBuscadorVideos")

            Dim gv As AdaptiveGridView = pagina.FindName("gvTiles")
            gv.DesiredWidth = anchoColumna
            gv.Items.Clear()

            Dim wv As WebView = sender
            Dim enlace As String = wv.Source.AbsoluteUri

            If enlace.Contains("primevideo.com/search/") Then
                Dim html As String = Await wv.InvokeScriptAsync("eval", New String() {"document.documentElement.outerHTML;"})

                If html.Contains("id=" + ChrW(34) + "av-search" + ChrW(34)) Then
                    Dim int As Integer = html.IndexOf("id=" + ChrW(34) + "av-search" + ChrW(34))
                    Dim temp As String = html.Remove(0, int)

                    Dim listaTiles As New List(Of Tile)

                    Dim i As Integer = 0
                    While i < 100
                        If temp.Contains("class=" + ChrW(34) + "av-hover-wrapper" + ChrW(34)) Then
                            Dim int2 As Integer = temp.IndexOf("class=" + ChrW(34) + "av-hover-wrapper" + ChrW(34))
                            Dim temp2 As String = temp.Remove(0, int2 + 2)

                            temp = temp2

                            Dim int3 As Integer = temp2.IndexOf("<a")
                            Dim temp3 As String = temp2.Remove(0, int3 + 2)

                            Dim int4 As Integer = temp3.IndexOf("href=")
                            Dim temp4 As String = temp3.Remove(0, int4 + 6)

                            Dim int5 As Integer = temp4.IndexOf(ChrW(34))
                            Dim temp5 As String = temp4.Remove(int5, temp4.Length - int5)

                            temp5 = temp5.Replace("/detail/", Nothing)

                            If temp5.Contains("/") Then
                                Dim int6 As Integer = temp5.IndexOf("/")
                                temp5 = temp5.Remove(int6, temp5.Length - int6)
                            End If

                            Dim id As String = temp5.Trim

                            Dim int7 As Integer = temp2.IndexOf("<img")
                            Dim temp7 As String = temp2.Remove(0, int7)

                            Dim int8 As Integer = temp7.IndexOf("src=")
                            Dim temp8 As String = temp7.Remove(0, int8 + 5)

                            Dim int9 As Integer = temp8.IndexOf(ChrW(34))
                            Dim temp9 As String = temp8.Remove(int9, temp8.Length - int9)

                            Dim imagen As String = temp9.Trim

                            Dim int10 As Integer = temp2.IndexOf("alt=")
                            Dim temp10 As String = temp2.Remove(0, int10 + 5)

                            Dim int11 As Integer = temp10.IndexOf(ChrW(34))
                            Dim temp11 As String = temp10.Remove(int11, temp10.Length - int11)

                            Dim titulo As String = temp11.Trim

                            imagen = Await Configuracion.Cache.DescargarImagen(imagen, id, "imagen")

                            Dim video As New Tile(titulo, id, "primevideo://app/detail?asin=" + id, imagen, imagen, imagen, imagen)
                            Dim añadir As Boolean = True

                            If listaTiles.Count > 0 Then
                                For Each tile In listaTiles
                                    If video.ID = tile.ID Then
                                        añadir = False
                                    End If
                                Next
                            End If

                            If añadir = True Then
                                listaTiles.Add(video)
                            End If
                        End If
                        i += 1
                    End While

                    GenerarTiles(gv, listaTiles)
                End If

                Estado(True, False)
            End If

        End Sub

        Private Sub Estado(estado As Boolean, borrarTexto As Boolean)

            Dim frame As Frame = Window.Current.Content
            Dim pagina As Page = frame.Content

            Dim botonBuscarVideos As Button = pagina.FindName("botonBuscarVideos")
            botonBuscarVideos.IsEnabled = estado

            Dim pbBuscadorVideos As ProgressBar = pagina.FindName("pbBuscadorVideos")

            If estado = False Then
                pbBuscadorVideos.Visibility = Visibility.Visible
            Else
                pbBuscadorVideos.Visibility = Visibility.Collapsed
            End If

            Dim tbBuscadorVideos As TextBox = pagina.FindName("tbBuscadorVideos")
            tbBuscadorVideos.IsEnabled = estado

            If borrarTexto = True Then
                tbBuscadorVideos.Text = String.Empty
            End If

        End Sub

        Private Sub GenerarTiles(gv As AdaptiveGridView, lista As List(Of Tile))

            For Each objeto In lista
                Dim panel As New DropShadowPanel With {
                    .Margin = New Thickness(10, 10, 10, 10),
                    .ShadowOpacity = 0.9,
                    .BlurRadius = 10,
                    .MaxWidth = anchoColumna + 20,
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }

                Dim boton As New Button

                Dim imagen As New ImageEx With {
                    .Source = objeto.ImagenGrande,
                    .IsCacheEnabled = True,
                    .Stretch = Stretch.UniformToFill,
                    .Padding = New Thickness(0, 0, 0, 0),
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center,
                    .EnableLazyLoading = True
                }

                boton.Tag = objeto
                boton.Content = imagen
                boton.Padding = New Thickness(0, 0, 0, 0)
                boton.Background = New SolidColorBrush(Colors.Transparent)

                panel.Content = boton

                Dim tbToolTip As TextBlock = New TextBlock With {
                    .Text = objeto.Titulo,
                    .FontSize = 16,
                    .TextWrapping = TextWrapping.Wrap
                }

                ToolTipService.SetToolTip(boton, tbToolTip)
                ToolTipService.SetPlacement(boton, PlacementMode.Mouse)

                AddHandler boton.Click, AddressOf Amazon.BotonTile_Click
                AddHandler boton.PointerEntered, AddressOf Entra_Boton_Imagen
                AddHandler boton.PointerExited, AddressOf Sale_Boton_Imagen

                gv.Items.Add(panel)
            Next

        End Sub

    End Module

End Namespace