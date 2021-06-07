<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication12.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Site Builder Interface</title>
    <h3 style="font-style: oblique; font-size: large;">Please fill in all the desired data</h3>
    <style type="text/css">
        #form1 {
            height: 540px;
            width: 1024px;
        }
    </style>
</head>
<body style="height: 657px">
    <form id="form1" runat="server" style="color: #000000; background-color: #c0d5d8; margin-top: 20px;" enableviewstate="True">
        <label for="dataname">Enter Database Name</label>
        <asp:TextBox ID="dataname" runat="server" Height="20px" OnTextChanged="dataname_TextChanged" Width="140px" AutoPostBack="True"></asp:TextBox>
        <label for="userTable_combo">Choose Name of Users Table</label>
        <asp:DropDownList ID="userTable_combo" runat="server" AutoPostBack="True" Height="20px" OnSelectedIndexChanged="userTable_combo_SelectedIndexChanged" Width="170px">
        </asp:DropDownList>
        <br />
        <label for="itemTable_combo">Choose Name of Items Table </label>
        <asp:DropDownList ID="itemTable_combo" runat="server" AutoPostBack="True" Height="20px" OnSelectedIndexChanged="itemTable_combo_SelectedIndexChanged" Style="margin-bottom: 13px" Width="136px">
        </asp:DropDownList>
        <label for="ratingTable_combo">Choose Name of Ratings Table</label>
        <asp:DropDownList ID="ratingTable_combo" runat="server" AutoPostBack="True" Height="20px" OnSelectedIndexChanged="ratingTable_combo_SelectedIndexChanged" Style="margin-bottom: 12px" Width="116px">
        </asp:DropDownList>
        <label for="MinSupport_txt">Set minimum support</label>
        <asp:TextBox ID="MinSupport_txt" runat="server" Height="20px" OnTextChanged="MinSupport_TextChanged" Style="margin-bottom: 12px" Width="137px" AutoPostBack="True"></asp:TextBox>
        <br />
        <br />
        <label for="UsersPK_drop">User Table PK</label>
        <asp:DropDownList ID="UsersPK_drop" runat="server" AutoPostBack="True" Height="20px" OnSelectedIndexChanged="UsersPK_drop_SelectedIndexChanged" Width="120px">
        </asp:DropDownList>
        <br />
        <label for="ItemsPK_drop">Items Table PK</label>
        <asp:DropDownList ID="ItemsPK_drop" runat="server" AutoPostBack="True" Height="20px" OnSelectedIndexChanged="ItemsPK_drop_SelectedIndexChanged" Width="120px">
        </asp:DropDownList>
        <br />
        <br />
        <br />
        <label for="ItemsFK_drop">FK of Items in Rating Table</label>
        <asp:DropDownList ID="ItemsFK_drop" runat="server" AutoPostBack="True" Height="20px" OnSelectedIndexChanged="ItemsFK_drop_SelectedIndexChanged" Width="120px">
        </asp:DropDownList>
        <br />
        <label for="UsersFK_drop">FK of Users in Rating Table</label>
        <asp:DropDownList ID="UsersFK_drop" runat="server" AutoPostBack="True" Height="20px" OnSelectedIndexChanged="UsersFK_drop_SelectedIndexChanged" Width="120px">
        </asp:DropDownList>
        <br />
        <label for="rateN_drop">Column name of Ratings</label>
        <asp:DropDownList ID="rateN_drop" runat="server" AutoPostBack="True" Height="20px" Width="120px">
        </asp:DropDownList>
        <br />
        <br />
        <br />
        <asp:ListBox ID="dbContexts_List" runat="server" AutoPostBack="True" Height="216px" OnSelectedIndexChanged="dbContexts_List_SelectedIndexChanged" SelectionMode="Multiple" Width="278px"></asp:ListBox>
        &nbsp; &nbsp; &nbsp; 
        <asp:ListBox CssClass="ClassA" ID="chosenContexts_List" runat="server" AutoPostBack="True" Height="216px" SelectionMode="Multiple" Width="278px"></asp:ListBox>
        <asp:Button CssClass="ClassA" ID="clrBtn" runat="server" Text="Clear Choosen Contexts" Height="28px" Width="100px" OnClick="clrBtn_OnClick" style="background-color:#d1d39f;" />
        <br />
        <asp:Button ID="btnStart"  runat="server" Text="Click To Start" Height="28px" Width="100px" OnClick="btnStart_OnClick" Style="margin-left: 250px; margin-top: 10px;" />
    </form>
</body>
</html>
