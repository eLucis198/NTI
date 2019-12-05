using CentralR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CentralR.Controllers
{
    public class LoginController : Controller
    {
        CentralREntities2 db = new CentralREntities2();

        // GET: Login
        public ActionResult Index()
        {
            if (Session["Logado"] != null)
            {
                if ((bool)Session["Logado"] == true)
                {
                    return RedirectToAction("Upload", "Cliente");
                }
            }
            if (Request.Cookies["cLogin"] != null)
            {
                ViewBag.CPF = Request.Cookies["cLogin"]["CPF"];
                ViewBag.Senha = Request.Cookies["cLogin"]["Senha"];
                ViewBag.Lembrar = true;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(string txtCpf, string txtSenha, bool cbxLembrar)
        {
            if (string.IsNullOrEmpty(txtCpf) || string.IsNullOrEmpty(txtSenha))
            {
                ModelState.AddModelError("", "Os campos CPF e Senha são obrigatórios");
            }
            else
            {
                Usuario usuario = new Usuario();
                usuario = db.Usuario.FirstOrDefault(x=> x.CPF.Equals(txtCpf) && x.Senha.Equals(txtSenha));
                if (usuario != null)
                {
                    Session["Logado"] = true;
                    if (usuario.Acesso == true)
                    {
                        Session["Administrador"] = true;
                    }
                    if (cbxLembrar == true)
                    {
                        HttpCookie cookie = new HttpCookie("cLogin");
                        cookie.Values.Add("CPF", txtCpf);
                        cookie.Values.Add("Senha", txtSenha);
                        cookie.Expires = DateTime.Now.AddHours(8);
                        Response.AppendCookie(cookie);
                    }
                    else if (cbxLembrar == false)
                    {
                        if (Request.Cookies["cLogin"] != null)
                        {
                            Response.Cookies["cLogin"].Expires = DateTime.Now.AddDays(-1);
                        }
                    }
                    return RedirectToAction("Upload", "Cliente");
                }
                else
                {
                    ModelState.AddModelError("", "CPF ou senha inválidos");
                }
            }
            return View();
        }
    }
}