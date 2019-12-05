using CentralR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CentralR.Controllers
{
    public class UsuarioController : Controller
    {
        CentralREntities2 db = new CentralREntities2();
        public ActionResult Index()
        {
            if (Session["Logado"] != null)
            {
                if ((bool)Session["Logado"] == true)
                {
                    if (Session["Administrador"] != null)
                    {
                        if ((bool)Session["Administrador"] == true)
                        {
                            return View();
                        }
                    }
                    return RedirectToAction("Upload", "Cliente");
                }
            }
            return RedirectToAction("Index", "Login");
        }

        public JsonResult Pesquisar(FormCollection form)
        {
            string pesquisa = form["txtPesquisa"];
            IList<Usuario> listaUsuarios = new List<Usuario>();
            listaUsuarios = db.Usuario.Where(x=> x.Nome.Contains(pesquisa) || x.CPF.Contains(pesquisa)).ToList();

            StringBuilder str = new StringBuilder();

            str.Append("<table class=\"table table-bordered table-striped\">");
            str.Append("<tr> <td><b>#</b></td> <td><b>ID</b></td> <td><b>NOME</b></td> <td><b>CPF</b></td> <td><b>ATIVO</b></td> <td><b>ADMIN</b></td> <td><b>EDITAR</b></td> </tr>");
            int y = 0;
            foreach (var item in listaUsuarios)
            {
                y++;
                str.Append("<tr> <td>" + y + "</td><td>" + item.ID_Usuario + "</td> <td>" + item.Nome + "</td> <td>" + item.CPF + "</td> <td>" + item.Status_ + "</td> <td>" + item.Acesso + "</td> <td> <a id=\"Editar\" onclick=\"Editar("+item.ID_Usuario+")\">EDITAR</a> </td> </tr>");
            }
            str.Append("</table>");

            return Json(new { 
                ListaUsuarios = str.ToString()
            },JsonRequestBehavior.AllowGet);
        }

        // *#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#

        public ActionResult Editar()
        {
            if (Session["Logado"] != null)
            {
                if ((bool)Session["Logado"] == true)
                {
                    if (Session["Administrador"] != null)
                    {
                        if ((bool)Session["Administrador"] == true)
                        {
                            int id = 0;
                            if (Session["IdEditar"] != null)
                            {
                                id = (int)Session["IdEditar"];
                            }
                            else
                            {
                                return RedirectToAction("Index", "Usuario");
                            }
                            Usuario usuario = new Usuario();
                            usuario = db.Usuario.Find(id);
                            return View(usuario);
                        }
                    }
                    return RedirectToAction("Upload", "Cliente");
                }
            }
            return RedirectToAction("Index", "Login");
        }

        public void SetaEditar(int id)
        {
            Session["IdEditar"] = id;
        }
    }
}