<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>WEB-Sklad</h1>
        <p class="lead">Вас вітає інформаційний ресурс для клієнтів ТОВ "Брусилвські ковбаси".</p>
        <p><a href="/" class="btn btn-primary btn-lg">Прайс лист &raquo;</a></p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Замовлення</h2>
            <p>
                Ваші замовлення.
            </p>
            <p>
                <a class="btn btn-default"  runat="server" href="~/Apps/MyOrders.aspx">Перейти &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Видаткові документи</h2>
            <p>
                Ваші видаткові документи.
            </p>
            <p>
                <a class="btn btn-default" href="Apps/MyPurchases.aspx">Перейти &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Оплата</h2>
            <p>
                Оплата по видаткавох документах.
            </p>
            <p>
                <a class="btn btn-default" href="~/">Перейти &raquo;</a>
            </p>
        </div>
    </div>

</asp:Content>
