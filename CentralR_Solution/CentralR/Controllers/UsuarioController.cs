using CentralR.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CentralR.Controllers
{
    public class UsuarioController : Controller
    {
        CentralREntities2 db = new CentralREntities2();

        // *#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#

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
                str.Append("<tr> <td>" + y + "</td><td>" + item.ID_Usuario + "</td> <td>" + item.Nome + "</td> <td>" + item.CPF + "</td> <td>" + (item.Status_==true?"Ativo":"Inativo") + "</td> <td>" + (item.Administrador == true ? "Sim" : "Não") + "</td> <td> <a id=\"Editar\" onclick=\"Editar("+item.ID_Usuario+")\">EDITAR</a> </td> </tr>");
            }
            str.Append("</table>");

            return Json(new { 
                ListaUsuarios = str.ToString()
            },JsonRequestBehavior.AllowGet);
        }

        // *#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#

        public ActionResult Novo()
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

        [HttpPost]
        public ActionResult Novo(Usuario usuario)
        {
            if (validaCpf(usuario.CPF))
            {
                try
                {
                    db.Usuario.Add(usuario);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Usuario");
                }
                catch (DbEntityValidationException ex)
                {
                    var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                    this.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("CPF", "CPF Inválido");
                return View();
            }
        }

        public bool validaCpf(string cpf) {
            if (string.IsNullOrEmpty(cpf))
            {
                ModelState.AddModelError("CPF", "CPF Inválido");
                return false;
            }
            if (cpf.Length != 14 || cpf.Contains("_"))
            {
                ModelState.AddModelError("CPF", "CPF Inválido");
                return false;
            }
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            return tempCpf.Equals(cpf)?true:false;
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

        [HttpPost]
        public ActionResult Editar(Usuario usuario)
        {
            Usuario u = db.Usuario.Find(usuario.ID_Usuario);
            u.Nome = usuario.Nome;
            u.CPF = usuario.CPF;
            u.Senha = usuario.Senha;
            u.Status_ = usuario.Status_;
            u.Administrador = usuario.Administrador;
            db.Entry(u).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", "Usuario");
        }

        public void SetaEditar(int id)
        {
            Session["IdEditar"] = id;
        }

        // *#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#
    }
}