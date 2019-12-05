using CentralR.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CentralR.Controllers
{
    public class AgendamentoConsultaController : Controller
    {
        CentralREntities2 db = new CentralREntities2();

        // GET: AgendamentoConsulta
        public ActionResult Upload()
        {
            if (Session["Logado"] != null)
            {
                if ((bool)Session["Logado"] == true)
                {
                    return View();
                }
            }
            return RedirectToAction("Index", "Login");
        }

        public JsonResult UploadAgendamentoConsulta(FormCollection form)
        {
            string status = "";
            int contador = -1;
            DateTime tempoInicio = DateTime.Now;
            DateTime tempoFim = DateTime.Now;
            TimeSpan tempoTotal = tempoFim.Subtract(tempoInicio);
            HttpPostedFileBase file = Request.Files[0];

            if (file != null)
            {
                FileInfo fileInfo = new FileInfo(file.FileName);
                if (fileInfo.Extension == ".csv" && file.ContentLength > 0 && file.ContentLength < 52428800) //52428800 == 50MB
                {
                    try
                    {
                        tempoInicio = DateTime.Now;
                        contador = LerArquivoConsulta(file);
                        tempoFim = DateTime.Now;
                        tempoTotal = tempoFim.Subtract(tempoInicio);
                        status = "Sucesso";
                    }
                    catch (Exception e)
                    {
                        status = "Erro ao ler o arquivo\n"+e.StackTrace;
                    }
                }
                else { status = "Arquivo Inválido"; }
            }else { status = "Arquivo Inválido"; }

            return Json(new
            {
                Status = status,
                Contador = contador,
                Tempo = tempoTotal.ToString("mm\\:ss\\:ff")
            }, JsonRequestBehavior.AllowGet);
        }

        public int LerArquivoConsulta(HttpPostedFileBase file)
        {
            int contador = 0;
            // Criando o Encoding para ler ISO-8859-1 pois esse é o formato do arquivo .csv
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            // Em vez de salvar o arquivo no servidor, e após isso ler ele de dentro do servidor
            // é feita a conversão do arquivo "Upload" para bytes diretamente, e após isso para uma string com o leitor ISO-8859-1
            string resultado = string.Empty;
            using (BinaryReader rdr = new BinaryReader(file.InputStream))
            {
                byte[] csvBytes = rdr.ReadBytes(file.ContentLength);
                // Essa variável armazena todo o texto do arquivo .csv de agendamento de pacientes
                resultado = iso.GetString(csvBytes);
            }

            // Divide o texto resultado em linhas pela quebra de linha '\n'
            string[] line = resultado.Split('\n');

            // For percorre todas as linhas do arquivo
            for (int i = 0; i < line.Length; i++)
            {
                Debug.WriteLine("Linha: "+ i);
                // Armazena todos os valores q a linha contem na variavel vetor valor[]
                string[] conteudo = line[i].Split(';');


                // Verificando se contém mais de um valor ou se é o cabeçalho
                if (!(conteudo.Length <= 1) && !(conteudo[1].Contains("Data da Consulta")) && (int.TryParse(conteudo[4], out _)))
                {
                    try
                    {
                        AgendamentoConsulta agendamentoC = new AgendamentoConsulta();
                        Cliente c = db.Cliente.Find(Convert.ToInt32(conteudo[4]));
                        for (int j = 0; j < conteudo.Length; j++)
                        {
                            // Dando um Trim() em todos os valores da linha para que não tenha um espaço no inicio de cada valor
                            conteudo[j] = conteudo[j].Trim();
                        }
                        //  outra maneira de se tentar -> DateTime dateTemp1 = Convert.ToDateTime(valor[1] + " " + valor[3]);
                        // Merge em data e hora da consulta e conversão para o tipo DateTime no formato "dd/MM/yyyy HH:mm"
                        DateTime dateTemp1 = DateTime.ParseExact(conteudo[1], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime dateTemp2 = DateTime.ParseExact(conteudo[2], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        DateTime hourTemp1 = DateTime.ParseExact(conteudo[3], "HH:mm", CultureInfo.InvariantCulture);
                        agendamentoC.DataConsulta = dateTemp1;
                        agendamentoC.HoraConsulta = hourTemp1.TimeOfDay;
                        agendamentoC.DataAgendamento = dateTemp2;
                        agendamentoC.Cliente = c;
                        agendamentoC.Prontuario = conteudo[6];
                        agendamentoC.Telefone = conteudo[7];
                        agendamentoC.Celular = conteudo[8];
                        agendamentoC.UnidadePrestadora = conteudo[9];
                        agendamentoC.NomeProfissional = conteudo[10];
                        agendamentoC.Especialidade = conteudo[11];
                        agendamentoC.Tipo = conteudo[12];
                        agendamentoC.Situacao = conteudo[13];

                        db.AgendamentoConsulta.Add(agendamentoC);
                        db.SaveChanges();
                        contador++;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
            return contador;
        }

        // #*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*

        public ActionResult Search()
        {
            if (Session["Logado"] != null)
            {
                if ((bool)Session["Logado"] == true)
                {
                    return View();
                }
                return RedirectToAction("Index", "Login");
            }
            return RedirectToAction("Index", "Login");
        }

        public JsonResult Pesquisar(FormCollection form)
        {
            // Cria uma lista que possue todos os agendamentos de consulta, conforme a pesquisa
            IList<ViewAgendamentoConsulta> listaAgendamentoConsulta = PesquisaAgendamentosConsulta(form);
            //Cria a table HTML de agendamentos de consulta no back end e retorna ela para a view em formato JSON por request ajax.
            StringBuilder str = new StringBuilder();

            str.Append("<table class=\"table table-bordered table-striped\">");
            str.Append("<tr> <td><b>#</b></td> <td><b>UNIDADE DO PACIENTE</b></td> <td><b>NOME DO PACIENTE</b></td> <td><b>DATA CONSULTA</b></td> <td><b>HORA CONSULTA</b></td> <td><b>UNIDADE PRESTADORA</b></td> <td><b>PROFISSIONAL</b></td> <td><b>ESPECIALIDADE</b></td> <td><b>AGENDADO EM:</b></td> </tr>");
            int y = 0;
            foreach (var item in listaAgendamentoConsulta)
            {
                y++;
                str.Append("<tr> <td>" + y + "</td><td>" + item.Descricao + "</td> <td>" + item.Nome + "</td> <td style=\"background-color: #c8ffc8\"><b>" + item.DataConsulta.Value.ToShortDateString() + "</b></td> <td>" + item.HoraConsulta + "</td> <td>" + item.UnidadePrestadora + "</td> <td>" + item.NomeProfissional + "</td> <td>" + item.Especialidade + "</td> <td>" + item.DataAgendamento.Value.ToShortDateString() + "</td> </tr>");
            }

            str.Append("</table>");

            string Resultado = "<b>Resultados: " + listaAgendamentoConsulta.Count + "</b>";

            return Json(new
            {
                ListaDeAgendamentos = str.ToString(),
                Resultado
            }, JsonRequestBehavior.AllowGet);
        }

        public IList<ViewAgendamentoConsulta> PesquisaAgendamentosConsulta(FormCollection form)
        {
            ViewAgendamentoConsulta ac = new ViewAgendamentoConsulta();
            ac.Nome = form["txtNomeCliente"];
            ac.Descricao = form["txtUnidadeCliente"];
            ac.UnidadePrestadora = form["txtUnidadePrestadora"];
            ac.NomeProfissional = form["txtProfissional"];
            ac.Especialidade = form["txtEspecialidade"];

            StringBuilder str = new StringBuilder();
            DateTime dateConsultaFrom = new DateTime();
            DateTime dateConsultaTo = new DateTime();
            DateTime dateAgendamentoFrom = new DateTime();
            DateTime dateAgendamentoTo = new DateTime();
            DateTime timeConsultaFrom = new DateTime();
            DateTime timeConsultaTo = new DateTime();

            if (string.IsNullOrEmpty(form["txtDataConsultaFrom"]))
            {
                dateConsultaFrom = DateTime.Now.AddYears(-100);
            }
            else
            {
                dateConsultaFrom = DateTime.ParseExact(form["txtDataConsultaFrom"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(form["txtDataConsultaTo"]))
            {
                dateConsultaTo = DateTime.Now.AddYears(100);
            }
            else
            {
                dateConsultaTo = DateTime.ParseExact(form["txtDataConsultaTo"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(form["txtDataAgendamentoFrom"]))
            {
                dateAgendamentoFrom = DateTime.Now.AddYears(-100);
            }
            else
            {
                dateAgendamentoFrom = DateTime.ParseExact(form["txtDataAgendamentoFrom"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(form["txtDataAgendamentoTo"]))
            {
                dateAgendamentoTo = DateTime.Now.AddYears(100);
            }
            else
            {
                dateAgendamentoTo = DateTime.ParseExact(form["txtDataAgendamentoTo"], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(form["txtHoraConsultaFrom"]))
            {
                TimeSpan ts = new TimeSpan(00, 00, 00);
                timeConsultaFrom = timeConsultaFrom.Date + ts;
            }
            else
            {
                timeConsultaFrom = DateTime.ParseExact(form["txtHoraConsultaFrom"], "HH:mm", CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrEmpty(form["txtHoraConsultaTo"]))
            {
                TimeSpan ts = new TimeSpan(23, 59, 59);
                timeConsultaTo = timeConsultaTo.Date + ts;
            }
            else
            {
                timeConsultaTo = DateTime.ParseExact(form["txtHoraConsultaTo"], "HH:mm", CultureInfo.InvariantCulture);
            }

            IList<ViewAgendamentoConsulta> listaAgendamentoConsulta = new List<ViewAgendamentoConsulta>();
            listaAgendamentoConsulta = db.ViewAgendamentoConsulta.Where(x => x.Nome.Contains(ac.Nome) && x.Descricao.Contains(ac.Descricao) && x.UnidadePrestadora.Contains(ac.UnidadePrestadora) && x.NomeProfissional.Contains(ac.NomeProfissional) && x.Especialidade.Contains(ac.Especialidade) && x.DataConsulta >= dateConsultaFrom && x.DataConsulta <= dateConsultaTo && x.DataAgendamento >= dateAgendamentoFrom && x.DataAgendamento <= dateAgendamentoTo && x.HoraConsulta >= timeConsultaFrom.TimeOfDay && x.HoraConsulta <= timeConsultaTo.TimeOfDay).OrderBy(x => x.DataConsulta).ThenBy(x => x.HoraConsulta).ToList();

            return listaAgendamentoConsulta;
        }

        public JsonResult PesquisarUnidade()
        {
            List<string> lista = PesquisaUnidades();
            StringBuilder str = new StringBuilder();
            str.Append("<option value=\"\">Todos</option>");
            foreach (var item in lista)
            {
                str.Append("<option>" + item + "</option>");
            }

            return Json(new
            {
                ListaDeUnidades = str.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        public List<string> PesquisaUnidades()
        {
            List<string> listaUnidades = new List<string>();
            List<Unidade> unidades = new List<Unidade>();
            unidades = db.Unidade.ToList();
            foreach (var item in unidades)
            {
                listaUnidades.Add(item.Descricao);
            }
            return listaUnidades;
        }

        // #*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*#*

        public ActionResult CreateFile(FormCollection form)
        {
            // Crio uma lista com todos os agendamentos da consulta
            IList<ViewAgendamentoConsulta> listaAgendamentoConsulta = PesquisaAgendamentosConsulta(form);
            string handle = Guid.NewGuid().ToString();
            // Pego todas as unidades da lista, para que o arquivo possua uma planilha por unidade 
            // Unidades é um vetor string que contém o nome de todas as unidades
            List<string> Unidades = new List<string>();
            foreach (var item in listaAgendamentoConsulta)
            {
                if (!Unidades.Contains(item.Descricao))
                {
                    Unidades.Add(item.Descricao);
                }
            }

            // Veifica se possui pelo menos um agendamento na Lista
            if (listaAgendamentoConsulta.Count >= 1)
            {
                // Excel package será o arquivo .xlsx
                using (ExcelPackage ArquivoExcel = new ExcelPackage())
                {
                    // Dividirá o arquivo pela unidade dos clientes
                    foreach (var item in Unidades)
                    {
                        List<ViewAgendamentoConsulta> listOrdered = new List<ViewAgendamentoConsulta>();
                        // foreach serve para o seguinte, o listOrdered irá armazenar as pessoas de certa unidade;
                        // A variável item possui uma string com o nome da unidade do paciente, será verificado se o cliente do agendamento (listaAgendamentoConsulta) é da unidade (item)
                        // Se for será adicionado na listOrdered, assim listOrdered armazenará todos os agendamentos onde o cliente é da unidade atual que está sendo gravada
                        // Assim dividindo o arquivo por Unidades, cada planilha tendo sua unidade
                        foreach (var agendamento in listaAgendamentoConsulta)
                        {
                            if (agendamento.Descricao.Equals(item.ToString()))
                            {
                                listOrdered.Add(agendamento);
                            }
                        }
                        // Cria a planilha com o nome da unidade
                        var Planilha = ArquivoExcel.Workbook.Worksheets.Add(item.ToString());
                        var headerRow = 2;
                        // Escreve o nome da unidade na planilha como um titulo
                        Planilha.Cells[1, 1].Value = item.ToString();
                        // Cria o header da tabela de agendamentos
                        Planilha.Cells[headerRow, 1].Value = "#";
                        Planilha.Cells[headerRow, 2].Value = "Nome";
                        Planilha.Cells[headerRow, 3].Value = "Data da Consulta";
                        Planilha.Cells[headerRow, 4].Value = "Hora da Consulta";
                        Planilha.Cells[headerRow, 5].Value = "Unidade Prestadora";
                        Planilha.Cells[headerRow, 6].Value = "Profissional";
                        Planilha.Cells[headerRow, 7].Value = "Especialidade";

                        // irá ler os agendamentos onde o cliente é da unidade, e para cada um deles irá gravar na linha os dados
                        for (int i = 0; i < listOrdered.Count; i++)
                        {
                            var row = i + (headerRow + 1);
                            Planilha.Cells[row, 1].Value = (i + 1).ToString();
                            Planilha.Cells[row, 2].Value = listOrdered[i].Nome;
                            Planilha.Cells[row, 3].Value = listOrdered[i].DataConsulta.Value.ToShortDateString();
                            Planilha.Cells[row, 4].Value = listOrdered[i].HoraConsulta.ToString();
                            Planilha.Cells[row, 5].Value = listOrdered[i].UnidadePrestadora;
                            Planilha.Cells[row, 6].Value = listOrdered[i].NomeProfissional;
                            Planilha.Cells[row, 7].Value = listOrdered[i].Especialidade;
                        }

                        // *********Parte do estilo da Planilha

                        //Estilo do título
                        // unidadeRange Armazena o "Range" do nome da unidade, ou seja, de qual célula a qual célula vai o nome do titulo(nome da unidade) da planilha
                        var unidadeRange = "A1:G1";
                        Planilha.Row(1).Height = 32;
                        Planilha.Cells[unidadeRange].Style.Font.Bold = true;
                        Planilha.Cells[unidadeRange].Style.Font.Size = 24;
                        Planilha.Cells[unidadeRange].Merge = true;
                        Planilha.Cells[unidadeRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Planilha.Cells[unidadeRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        Planilha.Cells[unidadeRange].Style.Border.BorderAround(ExcelBorderStyle.Thick, Color.Black);

                        // Estilo do cabeçalho da tabela
                        var headerRange = "A" + headerRow + ":G" + headerRow;
                        Planilha.Row(headerRow).Height = 35;
                        Planilha.Cells[headerRange].Style.Font.Bold = true;
                        Planilha.Cells[headerRange].Style.Font.Size = 10;
                        Planilha.Cells[headerRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        Planilha.Cells[headerRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        Planilha.Cells[headerRange].Style.WrapText = true;

                        // Estilo dos dados da tabela
                        var tableRange = "A" + (headerRow + 1) + ":G1000";
                        Planilha.Column(1).Width = 3;
                        Planilha.Column(2).Width = 18;
                        Planilha.Column(3).Width = 10;
                        Planilha.Column(4).Width = 8;
                        Planilha.Column(5).Width = 22;
                        Planilha.Column(6).Width = 18;
                        Planilha.Column(7).Width = 18;
                        Planilha.Cells[tableRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        Planilha.Cells[tableRange].Style.Font.Size = 9;
                        Planilha.Cells[tableRange].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        Planilha.Cells[tableRange].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        Planilha.Cells[tableRange].Style.WrapText = true;
                        for (int i = headerRow + 1; i <= 1000; i++)
                        {
                            Planilha.Row(i).Height = 32;
                        }
                        Planilha.Cells[headerRange].Style.Border.BorderAround(ExcelBorderStyle.Medium, Color.Black);

                        //Configurações de impressão
                        Planilha.PrinterSettings.Orientation = eOrientation.Landscape;
                        Planilha.Column(7).PageBreak = true;
                    }

                    // Grava o arquivo em uma MemoryStream
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        ArquivoExcel.SaveAs(memoryStream);
                        memoryStream.Position = 0;
                        TempData[handle] = memoryStream.ToArray();
                    }
                }
            }

            return new JsonResult()
            {
                Data = new { FileGuid = handle, FileName = "Agendamento De Consultas.xlsx" }
            };
        }

        [HttpGet]
        public virtual ActionResult Download(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.ms-excel", fileName);
            }
            else
            {
                return new EmptyResult();
            }
        }
    }
}