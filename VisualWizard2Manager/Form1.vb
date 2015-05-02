Imports System.Xml
Imports ICSharpCode.SharpZipLib
Imports SimpleSerials
Imports System.IO

Public Class Form1

    Dim wizzip As New ICSharpCode.SharpZipLib.Zip.FastZip
    Dim lastfname As String
    Dim VSTools As VSSharedSource = New VSSharedSource
    Dim releasefolder As String
    Dim errprc As Integer
    Dim editingupdchan As Integer
    Dim CreatePatch As Integer

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ComboBox1.SelectedIndex = 0
        Label1.Text = "Registered to: " & Environment.UserName
        Label2.Text = Environment.MachineName
        If My.Application.Info.CompanyName = "Visual Software" Then
            If Now.Year > 2013 Then
                linklabel1.Text = My.Application.Info.AssemblyName & " © 2013-" & Now.Year & " " & My.Application.Info.CompanyName
            Else
                linklabel1.Text = My.Application.Info.AssemblyName & " © 2013" & " " & My.Application.Info.CompanyName
            End If
        Else
            MessageBox.Show("Error, This application has been modified", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End
        End If
        label8.Text = "Version " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build & "." & My.Application.Info.Version.Revision
        Try
            If IO.File.Exists(My.Application.Info.DirectoryPath & "\Setup.upd") = True Then
                If IO.File.Exists(My.Application.Info.DirectoryPath & "\Setup.exe") = True Then
                    IO.File.Delete(My.Application.Info.DirectoryPath & "\Setup.exe")
                Else

                End If
                My.Computer.FileSystem.RenameFile(My.Application.Info.DirectoryPath & "\Setup.upd", "Setup.exe")
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub MetroTextBox1_TextChanged(sender As Object, e As EventArgs) Handles MetroTextBox1.TextChanged
        MetroTextBox2.Text = MetroTextBox1.Text.Replace(" ", "") & ".pkg"
    End Sub

    Private Sub MetroButton1_Click(sender As Object, e As EventArgs) Handles MetroButton1.Click
        OpenFileDialog2.ShowDialog()
        If OpenFileDialog2.FileName = "" Then

        Else
            ListView1.Items.Clear()
            MetroButton2.Enabled = True
            PictureBox4.Enabled = True
            MetroTextBox9.Text = ""
            LoadXMLSettings(OpenFileDialog2.FileName)
            lastfname = OpenFileDialog2.FileName
        End If
    End Sub

    Private Sub MetroButton2_Click(sender As Object, e As EventArgs) Handles MetroButton2.Click
        SaveXMLSettings(lastfname, False)
        MessageBox.Show("Done", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Function LoadXMLSettings(ByVal FileName As String)
        Try
            Dim doc As New XmlDocument
            doc.Load(FileName)
            Dim nodes As XmlNodeList = doc.SelectNodes("VISUALWIZARD2/SetupConfig")
            For Each node As XmlNode In nodes
                'FILE VERSION 1.0 ################################
                'BASIC
                MetroTextBox1.Text = node.SelectSingleNode("AppName").InnerText
                MetroTextBox2.Text = node.SelectSingleNode("PackageName").InnerText
                MetroTextBox3.Text = node.SelectSingleNode("ExecutableName").InnerText
                ComboBox1.SelectedIndex = node.SelectSingleNode("UIColor").InnerText
                If node.SelectSingleNode("RunAppWhenFinish").InnerText = "1" Then
                    MetroCheckBox1.Checked = True
                Else
                    MetroCheckBox1.Checked = False
                End If
                If node.SelectSingleNode("PortableApp").InnerText = "1" Then
                    MetroCheckBox2.Checked = True
                Else
                    MetroCheckBox2.Checked = False
                End If
                'ADVANCED
                If node.SelectSingleNode("ShowEULA").InnerText = "1" Then
                    MetroCheckBox3.Checked = True
                    MetroTextBox4.Text = node.SelectSingleNode("EULAFile").InnerText
                Else
                    MetroCheckBox3.Checked = False
                    MetroTextBox4.Text = ""
                End If
                If node.SelectSingleNode("ShowPKEY").InnerText = "1" Then
                    MetroCheckBox4.Checked = True
                    'decryption code
                    Dim dec As Encriptador
                    dec = New Encriptador()
                    MetroTextBox5.Text = dec.DesEncriptarCadena(node.SelectSingleNode("KeyVar1").InnerText)
                    'end of decryption code
                    MetroTextBox6.Text = node.SelectSingleNode("KeyVar2").InnerText
                Else
                    MetroCheckBox4.Checked = False
                    MetroTextBox5.Text = ""
                    MetroTextBox6.Text = ""
                End If
                If node.SelectSingleNode("OnlineSetup").InnerText = "1" Then
                    MetroCheckBox5.Checked = True
                    MetroTextBox7.Text = node.SelectSingleNode("SetupURL").InnerText
                Else
                    MetroCheckBox5.Checked = False
                End If
                MetroTextBox8.Text = node.SelectSingleNode("FilesURL").InnerText
                'FILE VERSION 1.1 ################################
                'BASIC
                If node.SelectSingleNode("FileFormatRevision").InnerText > "2" Then
                    MessageBox.Show("Warning: This project file was created by a newer version of Visual Wizard 2, modifying it will cause a data loss", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If
                MetroTextBox14.Text = node.SelectSingleNode("AppVersion").InnerText
                MetroTextBox15.Text = node.SelectSingleNode("DevName").InnerText
                MetroTextBox16.Text = node.SelectSingleNode("AppWebsite").InnerText
                'UPDATES
                If node.SelectSingleNode("EnableUpdate").InnerText = "1" Then
                    MetroCheckBox6.Checked = True
                    MetroTextBox13.Text = node.SelectSingleNode("UpdateURL").InnerText
                    Dim nodes2 As XmlNodeList = doc.SelectNodes("VISUALWIZARD2/UpdateConfig/UpdateChannel")
                    For Each node1 As XmlNode In nodes2
                        Dim UCName As String = node1.SelectSingleNode("Name").InnerText
                        Dim UCPWD As String = node1.SelectSingleNode("Password").InnerText
                        Dim UCURL As String = node1.SelectSingleNode("URL").InnerText
                        ListView1.Items.Add(New ListViewItem(New String() {UCName, UCPWD, UCURL}))
                    Next
                Else
                    MetroCheckBox6.Checked = False
                End If
            Next
        Catch ex As Exception

        End Try
    End Function

    Function SaveXMLSettings(ByVal FileName As String, ByVal RedistributableVersion As Boolean)
        Dim doc As New XmlDocument()
        Dim mainroot As XmlElement = doc.CreateElement("VISUALWIZARD2")
        'SETUPCONFIG
        Dim root As XmlElement = doc.CreateElement("SetupConfig")
        Dim fileformatrevision As XmlElement = doc.CreateElement("FileFormatRevision")
        fileformatrevision.InnerText = "2" 'VER 1.1
        Dim appname As XmlElement = doc.CreateElement("AppName")
        appname.InnerText = MetroTextBox1.Text
        Dim appver As XmlElement = doc.CreateElement("AppVersion")
        appver.InnerText = MetroTextBox14.Text
        Dim appdev As XmlElement = doc.CreateElement("DevName")
        appdev.InnerText = MetroTextBox15.Text
        Dim appweb As XmlElement = doc.CreateElement("AppWebsite")
        appweb.InnerText = MetroTextBox16.Text
        Dim pkgname As XmlElement = doc.CreateElement("PackageName")
        pkgname.InnerText = MetroTextBox2.Text
        Dim exename As XmlElement = doc.CreateElement("ExecutableName")
        exename.InnerText = MetroTextBox3.Text
        Dim uicolor As XmlElement = doc.CreateElement("UIColor")
        uicolor.InnerText = ComboBox1.SelectedIndex
        Dim runappwhenfinish As XmlElement = doc.CreateElement("RunAppWhenFinish")
        runappwhenfinish.InnerText = MetroCheckBox1.CheckState
        Dim portableapp As XmlElement = doc.CreateElement("PortableApp")
        portableapp.InnerText = MetroCheckBox2.CheckState
        Dim showeula As XmlElement = doc.CreateElement("ShowEULA")
        showeula.InnerText = MetroCheckBox3.CheckState
        Dim eulafile As XmlElement = doc.CreateElement("EULAFile")
        If RedistributableVersion = True Then
            'Write EULA file contents
            If MetroTextBox4.Text = "" Then
                eulafile.InnerText = MetroTextBox4.Text
            Else
                eulafile.InnerText = IO.File.ReadAllText(MetroTextBox4.Text)
            End If
        Else
            'Write EULA file location
            eulafile.InnerText = MetroTextBox4.Text
        End If
        Dim showpkey As XmlElement = doc.CreateElement("ShowPKEY")
        showpkey.InnerText = MetroCheckBox4.CheckState
        'code encryption
        Dim enc As Encriptador
        enc = New Encriptador()
        Dim keyvar1 As XmlElement = doc.CreateElement("KeyVar1")
        keyvar1.InnerText = enc.EncriptarCadena(MetroTextBox5.Text)
        'end of code encryption
        Dim keyvar2 As XmlElement = doc.CreateElement("KeyVar2")
        keyvar2.InnerText = MetroTextBox6.Text
        Dim onlinesetup As XmlElement = doc.CreateElement("OnlineSetup")
        onlinesetup.InnerText = MetroCheckBox5.CheckState
        Dim setupurl As XmlElement = doc.CreateElement("SetupURL")
        setupurl.InnerText = MetroTextBox7.Text
        Dim filesurl As XmlElement = doc.CreateElement("FilesURL")
        filesurl.InnerText = MetroTextBox8.Text
        Dim updenabled As XmlElement = doc.CreateElement("EnableUpdate")
        updenabled.InnerText = MetroCheckBox6.CheckState
        Dim updserver As XmlElement = doc.CreateElement("UpdateURL")
        updserver.InnerText = MetroTextBox13.Text
        root.AppendChild(fileformatrevision)
        root.AppendChild(appname)
        root.AppendChild(appver)
        root.AppendChild(appdev)
        root.AppendChild(appweb)
        root.AppendChild(pkgname)
        root.AppendChild(exename)
        root.AppendChild(uicolor)
        root.AppendChild(runappwhenfinish)
        root.AppendChild(portableapp)
        root.AppendChild(showeula)
        root.AppendChild(eulafile)
        root.AppendChild(showpkey)
        root.AppendChild(keyvar1)
        root.AppendChild(keyvar2)
        root.AppendChild(onlinesetup)
        root.AppendChild(setupurl)
        root.AppendChild(filesurl)
        root.AppendChild(updenabled)
        root.AppendChild(updserver)
        mainroot.AppendChild(root)
        'UPDATECONFIG
        If MetroCheckBox6.Checked = True Then
            Dim branchnumber As Integer = 0
            Dim root2 As XmlElement = doc.CreateElement("UpdateConfig")
            For Each ListViewItem In ListView1.Items()
                If branchnumber > ListView1.Items.Count Then

                Else

                    Dim updchn As XmlElement = doc.CreateElement("UpdateChannel")
                    Dim branchname As XmlElement = doc.CreateElement("Name")
                    branchname.InnerText = ListView1.Items.Item(branchnumber).Text
                    updchn.AppendChild(branchname)
                    If RedistributableVersion = True Then
                        If ListView1.Items.Item(branchnumber).SubItems(1).Text = "" Then
                            'No password
                            Dim branchpassword As XmlElement = doc.CreateElement("Password")
                            branchpassword.InnerText = "No"
                            updchn.AppendChild(branchpassword)
                            Dim branchurl As XmlElement = doc.CreateElement("URL")
                            branchurl.InnerText = ListView1.Items.Item(branchnumber).SubItems(2).Text
                            updchn.AppendChild(branchurl)
                        Else
                            'Passworded
                            Dim branchpassword As XmlElement = doc.CreateElement("Password")
                            branchpassword.InnerText = "Yes"
                            updchn.AppendChild(branchpassword)
                            Dim plainText As String = ListView1.Items.Item(branchnumber).SubItems(2).Text
                            Dim password As String = ListView1.Items.Item(branchnumber).SubItems(1).Text
                            Dim wrapper As New VSSharedSource.Simple3Des(password)
                            Dim cipherText As String = wrapper.EncryptData(plainText)
                            Dim branchurl As XmlElement = doc.CreateElement("URL")
                            branchurl.InnerText = cipherText
                            updchn.AppendChild(branchurl)

                        End If
                    Else
                        Dim branchpassword As XmlElement = doc.CreateElement("Password")
                        branchpassword.InnerText = ListView1.Items.Item(branchnumber).SubItems(1).Text
                        updchn.AppendChild(branchpassword)
                        Dim branchurl As XmlElement = doc.CreateElement("URL")
                        branchurl.InnerText = ListView1.Items.Item(branchnumber).SubItems(2).Text
                        updchn.AppendChild(branchurl)
                    End If
                    root2.AppendChild(updchn)
                    branchnumber += 1
                End If
            Next
            mainroot.AppendChild(root2)
        Else

        End If
            'FINISH
            doc.AppendChild(mainroot)
            doc.Save(FileName)
    End Function
    Private Sub MetroButton3_Click(sender As Object, e As EventArgs) Handles MetroButton3.Click
        OpenFileDialog1.ShowDialog()
        If OpenFileDialog1.FileName = "" Then

        Else
            MetroTextBox4.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub MetroCheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles MetroCheckBox3.CheckedChanged
        If MetroCheckBox3.Checked = True Then
            MetroTextBox4.ReadOnly = False
            MetroButton3.Enabled = True
        Else
            MetroTextBox4.ReadOnly = True
            MetroButton3.Enabled = False
        End If
    End Sub

    Private Sub MetroCheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles MetroCheckBox4.CheckedChanged
        If MetroCheckBox3.Checked = True Then
            MetroTextBox5.ReadOnly = False
            MetroTextBox6.ReadOnly = False
            MetroButton7.Enabled = True
        Else
            MetroTextBox5.ReadOnly = True
            MetroTextBox6.ReadOnly = True
            MetroButton7.Enabled = False
        End If
    End Sub

    Private Sub MetroButton4_Click(sender As Object, e As EventArgs) Handles MetroButton4.Click
        FolderBrowserDialog1.ShowDialog()
        If FolderBrowserDialog1.SelectedPath = "" Then

        Else
            MetroTextBox8.Text = FolderBrowserDialog1.SelectedPath & "\"
        End If
    End Sub

    Private Sub MetroButton5_Click(sender As Object, e As EventArgs) Handles MetroButton5.Click
        If MetroCheckBox4.Checked = True Then
            wizzip.Password = MetroTextBox5.Text & "vw2"
        Else

        End If
        If CreatePatch = 0 Then
            releasefolder = "Release " & Now.Year & Now.Month & Now.Day & "-" & Now.Hour & Now.Minute
            Label3.Text = "Generating setup files..."

        Else
            releasefolder = "Update " & Now.Year & Now.Month & Now.Day & "-" & Now.Hour & Now.Minute
            Label3.Text = "Generating update files..."
        End If
        IO.Directory.CreateDirectory(releasefolder)
        MetroPanel1.Visible = True
        MetroTabControl1.Visible = False
        MetroProgressSpinner1.Value = 0
        NumericUpDown1.Value = 0
        errprc = 0
        Panel1.Enabled = False
        Timer2.Enabled = True
        BackgroundWorker2.RunWorkerAsync()
    End Sub

    Private Sub MetroButton6_Click(sender As Object, e As EventArgs) Handles MetroButton6.Click
        SaveFileDialog1.ShowDialog()
        If SaveFileDialog1.FileName = "" Then

        Else
            PictureBox4.Enabled = True
            SaveXMLSettings(SaveFileDialog1.FileName, False)
            lastfname = SaveFileDialog1.FileName
        End If
    End Sub

    Private Sub MetroCheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles MetroCheckBox5.CheckedChanged
        If MetroCheckBox5.Checked = True Then
            MetroTextBox7.ReadOnly = False
        Else
            MetroTextBox7.ReadOnly = True
        End If
    End Sub

    Private Sub MetroButton7_Click(sender As Object, e As EventArgs) Handles MetroButton7.Click
        Dim plainText As String = SimpleSerials.Serial.GenerateSerial(MetroTextBox5.Text, MetroTextBox6.Text)
        Dim password As String = MetroTextBox6.Text
        Dim wrapper As New VSSharedSource.Simple3Des(password)
        Dim cipherText As String = wrapper.EncryptData(plainText)
        MetroTextBox9.Text = cipherText
    End Sub

    Private Sub PictureBox14_Click(sender As Object, e As EventArgs) Handles PictureBox14.Click
        VSTools.OpenDonationPage()
    End Sub

    Private Sub PictureBox13_Click(sender As Object, e As EventArgs) Handles PictureBox13.Click
        Process.Start("https://www.twitter.com/VisualSoftCorp")
    End Sub

    Private Sub PictureBox10_Click(sender As Object, e As EventArgs) Handles PictureBox10.Click
        PictureBox10.BackColor = Color.DarkGreen
        MetroButton1_Click(sender, e)
    End Sub

    Private Sub PictureBox4_Click(sender As Object, e As EventArgs) Handles PictureBox4.Click
        PictureBox4.BackColor = Color.DarkGreen
        MetroButton2_Click(sender, e)
    End Sub

    Private Sub PictureBox6_Click(sender As Object, e As EventArgs) Handles PictureBox6.Click
        PictureBox6.BackColor = Color.DarkGreen
        MetroButton6_Click(sender, e)
    End Sub

    Private Sub PictureBox9_Click(sender As Object, e As EventArgs) Handles PictureBox9.Click
        PictureBox9.BackColor = Color.DarkGreen
        CreatePatch = 0
        MetroButton5_Click(sender, e)
    End Sub

    Private Sub PictureBox10_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox10.MouseLeave
        PictureBox10.BackColor = Color.SeaGreen
    End Sub

    Private Sub PictureBox10_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox10.MouseEnter
        PictureBox10.BackColor = Color.MediumSeaGreen
    End Sub

    Private Sub PictureBox4_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox4.MouseLeave
        PictureBox4.BackColor = Color.SeaGreen
    End Sub

    Private Sub PictureBox4_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox4.MouseEnter
        PictureBox4.BackColor = Color.MediumSeaGreen
    End Sub

    Private Sub PictureBox6_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox6.MouseEnter
        PictureBox6.BackColor = Color.MediumSeaGreen
    End Sub

    Private Sub PictureBox6_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox6.MouseLeave
        PictureBox6.BackColor = Color.SeaGreen
    End Sub

    Private Sub PictureBox9_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox9.MouseLeave
        PictureBox9.BackColor = Color.SeaGreen
    End Sub

    Private Sub PictureBox9_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox9.MouseEnter
        PictureBox9.BackColor = Color.MediumSeaGreen
    End Sub

    Private Sub linklabel1_Click(sender As Object, e As EventArgs) Handles linklabel1.Click
        Process.Start("http://visualsoftware.wordpress.com")
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If BackgroundWorker1.IsBusy = True Then

        Else
            BackgroundWorker1.RunWorkerAsync()
        End If
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork

    End Sub

    Private Sub BackgroundWorker2_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        Try
            wizzip.CreateZip(My.Application.Info.DirectoryPath & "\" & releasefolder & "\" & MetroTextBox2.Text, MetroTextBox8.Text, True, "")
        Catch ex As Exception
            errprc = 1
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BackgroundWorker2_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker2.RunWorkerCompleted
        Timer2.Enabled = False
        MetroProgressSpinner1.Value = 100
        If errprc = 1 Then

        Else
            If CreatePatch = 0 Then
                ' ### NEW SETUP ###
                SaveXMLSettings(My.Application.Info.DirectoryPath & "\Temp.xml", True)
                TextBox1.Text = IO.File.ReadAllText(My.Application.Info.DirectoryPath & "\Temp.xml")
                IO.File.Delete(My.Application.Info.DirectoryPath & "\Temp.xml")
                Dim enc As Encriptador
                enc = New Encriptador()
                TextBox1.Text = enc.EncriptarCadena(TextBox1.Text)
                IO.File.WriteAllText(My.Application.Info.DirectoryPath & "\" & releasefolder & "\Settings.dat", TextBox1.Text)
                'COPY SETUP STUFF
                If ComboBox1.SelectedIndex = 0 Then
                    'Green
                    IO.File.Copy(My.Application.Info.DirectoryPath & "\Setup.exe", My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.exe")
                ElseIf ComboBox1.SelectedIndex = 1 Then
                    'Blue
                    IO.File.Copy(My.Application.Info.DirectoryPath & "\Setup.exe", My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.dat")
                    My.Computer.FileSystem.WriteAllBytes(My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.exe", My.Resources.Blue, False)
                ElseIf ComboBox1.SelectedIndex = 2 Then
                    'Red
                    IO.File.Copy(My.Application.Info.DirectoryPath & "\Setup.exe", My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.dat")
                    My.Computer.FileSystem.WriteAllBytes(My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.exe", My.Resources.Red, False)
                ElseIf ComboBox1.SelectedIndex = 3 Then
                    'Yellow
                    IO.File.Copy(My.Application.Info.DirectoryPath & "\Setup.exe", My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.dat")
                    My.Computer.FileSystem.WriteAllBytes(My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.exe", My.Resources.Yellow, False)
                Else
                    'Purple
                    IO.File.Copy(My.Application.Info.DirectoryPath & "\Setup.exe", My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.dat")
                    My.Computer.FileSystem.WriteAllBytes(My.Application.Info.DirectoryPath & "\" & releasefolder & "\Setup.exe", My.Resources.Purple, False)
                End If
                IO.File.Copy(My.Application.Info.DirectoryPath & "\ICSharpCode.SharpZipLib.dll", My.Application.Info.DirectoryPath & "\" & releasefolder & "\ICSharpCode.SharpZipLib.dll")
                IO.File.Copy(My.Application.Info.DirectoryPath & "\SimpleSerials.dll", My.Application.Info.DirectoryPath & "\" & releasefolder & "\SimpleSerials.dll")
                Process.Start(My.Application.Info.DirectoryPath & "\" & releasefolder & "\")
                MessageBox.Show("Done", "Generate Setup", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                ' ### UPDATE/PATCH FILE ###
                SaveXMLUpdate(My.Application.Info.DirectoryPath & "\" & releasefolder & "\Update.xml")
                IO.File.WriteAllText(My.Application.Info.DirectoryPath & "\" & releasefolder & "\Version.txt", MetroTextBox14.Text)
                Process.Start(My.Application.Info.DirectoryPath & "\" & releasefolder & "\")
                MessageBox.Show("Done", "Generate Patch/Update", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End If
        MetroPanel1.Visible = False
        MetroTabControl1.Visible = True
        Panel1.Enabled = True
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If MetroProgressSpinner1.Value = "100" Then
            MetroProgressSpinner1.Value = "0"
            MetroProgressSpinner1.Value = MetroProgressSpinner1.Value + 1
        Else
            MetroProgressSpinner1.Value = MetroProgressSpinner1.Value + 1
        End If
        'CALC FILE SIZE
        Dim flsz As String
        Dim finalsize As String
        Dim fFile As New FileInfo(My.Application.Info.DirectoryPath & "\" & releasefolder & "\" & MetroTextBox2.Text)
        Dim fSize As Integer = fFile.Length
        Dim formattype As String
        flsz = Val(fSize) / 1024
        If Val(flsz) > 1000 Then
            finalsize = Val(flsz) / 1024
            formattype = " MB"
        Else
            finalsize = Val(flsz)
            formattype = " KB"
        End If
        NumericUpDown1.Value = Format(finalsize, "Fixed")
        Label4.Text = "Size: " & NumericUpDown1.Value & formattype
    End Sub

    Private Sub MetroTextBox9_TextChanged(sender As Object, e As EventArgs) Handles MetroTextBox9.TextChanged
        If MetroTextBox9.Text = "" Then
            MetroButton8.Enabled = False
        Else
            MetroButton8.Enabled = True
        End If
    End Sub

    Private Sub MetroButton8_Click(sender As Object, e As EventArgs) Handles MetroButton8.Click
        Clipboard.SetText(MetroTextBox9.Text)
    End Sub

    Private Sub MetroCheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles MetroCheckBox6.CheckedChanged
        If MetroCheckBox6.Checked = True Then
            MetroTextBox13.ReadOnly = False
            PictureBox2.Enabled = True
            MetroButton18.Enabled = True
            ListView1.Enabled = True
        Else
            MetroTextBox13.ReadOnly = True
            PictureBox2.Enabled = False
            MetroButton18.Enabled = False
            ListView1.Enabled = False
        End If
    End Sub

    Private Sub MetroButton10_Click(sender As Object, e As EventArgs) Handles MetroButton10.Click
        MetroPanel2.Visible = False
        MetroTextBox12.Text = ""
        MetroTextBox11.Text = ""
        MetroTextBox10.Text = ""
    End Sub

    Private Sub MetroButton9_Click(sender As Object, e As EventArgs) Handles MetroButton9.Click
        If editingupdchan = 1 Then
            Dim selected As ListViewItem = ListView1.SelectedItems(0)
            Dim indx As Integer = selected.Index
            Dim totl As Integer = ListView1.Items.Count
            selected.Text = MetroTextBox12.Text
            selected.SubItems(1).Text = MetroTextBox11.Text
            selected.SubItems(2).Text = MetroTextBox10.Text
        Else
            Dim newbranch As New ListViewItem
            newbranch.Text = MetroTextBox12.Text
            If MetroTextBox11.Text = "" Then
                'No password
                newbranch.SubItems.Add("")
            Else
                'Passworded
                newbranch.SubItems.Add(MetroTextBox11.Text)
            End If
            newbranch.SubItems.Add(MetroTextBox10.Text)
            ListView1.Items.Add(newbranch)
        End If
        MetroButton10_Click(sender, e)
    End Sub

    Private Sub MetroTextBox12_TextChanged(sender As Object, e As EventArgs) Handles MetroTextBox12.TextChanged
        If MetroTextBox12.Text = "" Then
            MetroButton9.Enabled = False
        Else
            MetroButton9.Enabled = True
        End If
    End Sub

    Private Sub MetroButton18_Click(sender As Object, e As EventArgs) Handles MetroButton18.Click
        editingupdchan = 0
        MetroPanel2.Visible = True
    End Sub

    Private Sub MetroButton15_Click(sender As Object, e As EventArgs) Handles MetroButton15.Click
        For Each i As ListViewItem In ListView1.SelectedItems
            ListView1.Items.Remove(i)
        Next
    End Sub

    Private Sub MetroButton20_Click(sender As Object, e As EventArgs) Handles MetroButton20.Click
        Try
            If ListView1.SelectedItems.Count > 0 Then
                Dim selected As ListViewItem = ListView1.SelectedItems(0)
                Dim indx As Integer = selected.Index
                Dim totl As Integer = ListView1.Items.Count

                If indx = totl - 1 Then
                    ListView1.Items.Remove(selected)
                    ListView1.Items.Insert(0, selected)
                Else
                    ListView1.Items.Remove(selected)
                    ListView1.Items.Insert(indx + 1, selected)
                End If
            Else
                MessageBox.Show("You can only move one item at a time. Please select only one item and try again.", "Item Select", MessageBoxButtons.OK, MessageBoxIcon.[Stop])
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub MetroButton19_Click(sender As Object, e As EventArgs) Handles MetroButton19.Click
        Try
            If ListView1.SelectedItems.Count > 0 Then
                Dim selected As ListViewItem = ListView1.SelectedItems(0)
                Dim indx As Integer = selected.Index
                Dim totl As Integer = ListView1.Items.Count

                If indx = 0 Then
                    ListView1.Items.Remove(selected)
                    ListView1.Items.Insert(totl - 1, selected)
                Else
                    ListView1.Items.Remove(selected)
                    ListView1.Items.Insert(indx - 1, selected)
                End If
            Else
                MessageBox.Show("You can only move one item at a time. Please select only one item and try again.", "Item Select", MessageBoxButtons.OK, MessageBoxIcon.[Stop])
            End If

        Catch ex As Exception
        End Try
    End Sub

    Private Sub EditChannelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EditChannelToolStripMenuItem.Click
        Dim selected As ListViewItem = ListView1.SelectedItems(0)
        Dim indx As Integer = selected.Index
        Dim totl As Integer = ListView1.Items.Count
        MetroTextBox12.Text = selected.Text
        MetroTextBox11.Text = selected.SubItems(1).Text
        MetroTextBox10.Text = selected.SubItems(2).Text
        editingupdchan = 1
        MetroPanel2.Visible = True
    End Sub

    Private Sub PictureBox11_Click(sender As Object, e As EventArgs) Handles PictureBox11.Click
        PictureBox11.BackColor = Color.DarkGreen
        If IO.File.Exists(My.Application.Info.DirectoryPath & "\Visual Wizard 2 Manager Help.pdf") = True Then
            Process.Start(My.Application.Info.DirectoryPath & "\Visual Wizard 2 Manager Help.pdf")
        Else
            MessageBox.Show("Error, the help file cannot be found. Reinstalling the application may fix it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub PictureBox11_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox11.MouseEnter
        PictureBox11.BackColor = Color.MediumSeaGreen
    End Sub

    Private Sub PictureBox11_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox11.MouseLeave
        PictureBox11.BackColor = Color.SeaGreen
    End Sub

    Private Sub PictureBox2_Click(sender As Object, e As EventArgs) Handles PictureBox2.Click
        PictureBox2.BackColor = Color.DarkGreen
        CreatePatch = 1
        MetroButton5_Click(sender, e)
    End Sub

    Private Sub PictureBox2_MouseEnter(sender As Object, e As EventArgs) Handles PictureBox2.MouseEnter
        PictureBox2.BackColor = Color.MediumSeaGreen
    End Sub

    Private Sub PictureBox2_MouseLeave(sender As Object, e As EventArgs) Handles PictureBox2.MouseLeave
        PictureBox2.BackColor = Color.SeaGreen
    End Sub

    Private Sub SaveXMLUpdate(FileName As String)
        Dim doc As New XmlDocument()
        Dim mainroot As XmlElement = doc.CreateElement("VISUALWIZARD2-UPDATECONFIG")
        Dim branchnumber As Integer = 0
        For Each ListViewItem In ListView1.Items()
            If branchnumber > ListView1.Items.Count Then

            Else

                Dim updchn As XmlElement = doc.CreateElement("UpdateChannel")
                Dim branchname As XmlElement = doc.CreateElement("Name")
                branchname.InnerText = ListView1.Items.Item(branchnumber).Text
                updchn.AppendChild(branchname)
                If ListView1.Items.Item(branchnumber).SubItems(1).Text = "" Then
                    'No password
                    Dim branchpassword As XmlElement = doc.CreateElement("Password")
                    branchpassword.InnerText = "No"
                    updchn.AppendChild(branchpassword)
                    Dim branchurl As XmlElement = doc.CreateElement("URL")
                    branchurl.InnerText = ListView1.Items.Item(branchnumber).SubItems(2).Text
                    updchn.AppendChild(branchurl)
                Else
                    'Passworded
                    Dim branchpassword As XmlElement = doc.CreateElement("Password")
                    branchpassword.InnerText = "Yes"
                    updchn.AppendChild(branchpassword)
                    Dim plainText As String = ListView1.Items.Item(branchnumber).SubItems(2).Text
                    Dim password As String = ListView1.Items.Item(branchnumber).SubItems(1).Text
                    Dim wrapper As New VSSharedSource.Simple3Des(password)
                    Dim cipherText As String = wrapper.EncryptData(plainText)
                    Dim branchurl As XmlElement = doc.CreateElement("URL")
                    branchurl.InnerText = cipherText
                    updchn.AppendChild(branchurl)

                End If
                mainroot.AppendChild(updchn)
                branchnumber += 1
            End If
        Next
        doc.AppendChild(mainroot)
        doc.Save(FileName)
    End Sub

    Private Sub MetroButton11_Click(sender As Object, e As EventArgs) Handles MetroButton11.Click

    End Sub
End Class

Public Class Encriptador
    Private patron_busqueda As String = "7wW$va8çpoLár}Ek¹iBN(unY%#½t<¼íKR4>óGeS¨Az;QD{6=¿/*yf1³cZ!?+@²gÑª9€®3'\ x´0P-MJ^j`ñ5»2_·©]X.OmT~ºU«H)|CI,Vhsbé¾§úF¡:l&¬[÷Çqd"
    Private Patron_encripta As String = "zOº[a:áÑGÇ÷45 Uv½n§tçc´¼{W>kfmb6.HZgR/#JN-íhyX\`^?,9%_F'¬©¿*~E$YSé!A<&«L;¨21Dexw²sªB}=@¹®ñIipj7ó]dl0¾8MrCT€)3Pq(ú¡+o»|³Q·KuV"


    Private Function EncriptarCaracter(ByVal caracter As String, _
    ByVal variable As Integer, _
    ByVal a_indice As Integer) As String

        Dim caracterEncriptado As String, indice As Integer

        If patron_busqueda.IndexOf(caracter) <> -1 Then
            indice = (patron_busqueda.IndexOf(caracter) + variable + a_indice) Mod patron_busqueda.Length
            Return Patron_encripta.Substring(indice, 1)
        End If

        Return caracter


    End Function

    Public Function DesEncriptarCadena(ByVal cadena As String) As String

        Dim idx As Integer
        Dim result As String

        For idx = 0 To cadena.Length - 1
            result += DesEncriptarCaracter(cadena.Substring(idx, 1), cadena.Length, idx)
        Next
        Return result
    End Function

    Private Function DesEncriptarCaracter(ByVal caracter As String, _
    ByVal variable As Integer, _
    ByVal a_indice As Integer) As String

        Dim indice As Integer

        If Patron_encripta.IndexOf(caracter) <> -1 Then
            If (Patron_encripta.IndexOf(caracter) - variable - a_indice) > 0 Then
                indice = (Patron_encripta.IndexOf(caracter) - variable - a_indice) Mod Patron_encripta.Length
            Else
                indice = (patron_busqueda.Length) + ((Patron_encripta.IndexOf(caracter) - variable - a_indice) Mod Patron_encripta.Length)
            End If
            indice = indice Mod Patron_encripta.Length
            Return patron_busqueda.Substring(indice, 1)
        Else
            Return caracter
        End If

    End Function

    Public Function EncriptarCadena(ByVal cadena As String) As String
        Dim idx As Integer
        Dim result As String

        For idx = 0 To cadena.Length - 1
            result += EncriptarCaracter(cadena.Substring(idx, 1), cadena.Length, idx)
        Next
        Return result

    End Function

End Class
