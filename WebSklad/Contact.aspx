<%@ Page Title="Контакти" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="WebSklad.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>Адреса:</h3>
    <address>
        ТОВ "Брусилівські ковбаси"<br />
        смт. Брусилів, вул. Небесної Сотні, 108<br />
        <abbr title="Phone">P:</abbr>
        0416231167
    </address>

    <address>
        <strong>Підтримка:</strong>   <a href="mailto:br.palamarchuk.sv@gmail.com">br.palamarchuk.sv@gmail.com</a><br />
        <strong>Маркетинг:</strong> <a href="mailto:megasmak@ukr.net">megasmak@ukr.net</a>
    </address>
</asp:Content>
