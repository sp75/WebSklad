using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApi.Controllers
{
    [RoutePrefix("api/settings")]
    public class RegController : ApiController
    {
        [HttpGet, Route("version")]
        public string GetVersion()
        {
            return "1.0.0.1";
        }

        [HttpGet, Route("kay-gen/{kay_id}")]
        public string Coding(String kay_id)
        {
            if (kay_id.Length < 4) return "Error code !";

            String conv = "", rezult = "";

            for (int a = 0; a < kay_id.Length; a++)   // цифри в групи символів
            {
                switch (kay_id[a])
                {
                    case '0': conv += "VER"; break;
                    case '1': conv += "G4F"; break;
                    case '2': conv += "BAL"; break;
                    case '3': conv += "PVS"; break;
                    case '4': conv += "ZHD"; break;
                    case '5': conv += "NSP"; break;
                    case '6': conv += "LEY"; break;
                    case '7': conv += "8RT"; break;
                    case '8': conv += "MF3"; break;
                    case '9': conv += "RUK"; break;
                }
            }

            for (int a = 1; conv.Length > a; a++)    // в кожній парі міняем символи місцями
            {
                rezult = rezult + conv[a] + conv[a - 1];
                a++;
            }

            if ((conv.Length - rezult.Length) == 1) rezult = rezult + conv[conv.Length - 1];  // корегування нечетности


            var arr = rezult.ToCharArray().Reverse().ToArray(); // Переворачиваем строку
            rezult = new string(arr);


            for (int a = rezult.Length / 3 - 1; a > 0; a--) // добавляем "-" через кожні три символи
            {
                rezult = rezult.Insert((a * 3), "-");
            }

            return rezult;
        }
    }
}
