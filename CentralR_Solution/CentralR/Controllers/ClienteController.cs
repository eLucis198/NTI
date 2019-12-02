using CentralR.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace CentralR.Controllers
{
    public class ClienteController : Controller
    {

        CentralREntities2 db = new CentralREntities2();
        // GET: Cliente
        public ActionResult Upload()
        {
            return View();
        }

        public JsonResult UploadCliente(FormCollection form)
        {
            string status = "";
            int contador = -1;
            DateTime tempoInicio;
            DateTime tempoFim;
            TimeSpan tempoTotal = DateTime.Now.TimeOfDay;
            HttpPostedFileBase file = Request.Files[0];

            if (file != null)
            {
                FileInfo fileInfo = new FileInfo(file.FileName);
                if (fileInfo.Extension == ".pdf" && file.ContentLength > 0 && file.ContentLength < 52428800) //52428800 == 50MB
                {
                    try
                    {
                        tempoInicio = DateTime.Now;
                        contador = LerArquivoCliente(file);
                        tempoFim = DateTime.Now;
                        tempoTotal = tempoFim.Subtract(tempoInicio);
                        status = "Sucesso";
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                } else { status = "Arquivo muito grande"; }
            } else { status = "Error"; }

            return Json(new
            {
                Status = status,
                Contador = contador,
                Tempo = tempoTotal.ToString("mm\\:ss\\:ff")
            }, JsonRequestBehavior.AllowGet);
        }

        public int LerArquivoCliente(HttpPostedFileBase file)
        {
            //contador serve para contar o numero de clientes inseridos
            int contador = 0;

            byte[] pdfBytes = null;
            BinaryReader rdr = new BinaryReader(file.InputStream);
            pdfBytes = rdr.ReadBytes((int)file.ContentLength);
            // Le o arquivo dentro de um using
            using (PdfReader reader = new PdfReader(pdfBytes))
            {
                // contador de inserts, idUnidade e nomeUnidade são variáveis que passão de página para página
                int idUnidade = 0;
                string nomeUnidade = "";
                string situacao = "";

                // pdf é o arquivo, pageNum a quantidade de páginas do arquivo
                int pageNum = reader.NumberOfPages;

                //lines[] conterá cada linha da página atual
                string[] lines;

                // for que le o arquivo página por página
                for (int i = 1; i <= pageNum; i++)
                {
                    // Mostra no console do visual studio que página está
                    Debug.WriteLine("Página: " + i + " / " + pageNum);
                    //pageText armazena todo o texto de uma página
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                    //lines[] armazena o texto da página separado por linha
                    lines = pageText.Split('\n');
                    for (int j = 0; j < lines.Length; j++)
                    {
                        //conteudo armazenha a linha separada por espaços
                        string[] conteudo = lines[j].Split(' ');


                        if (lines[j].Contains("Situação:"))
                        {
                            if (conteudo.Count() == 2)
                            {
                                situacao = conteudo[1];
                            }
                            else if(conteudo.Count() == 3 && lines[j].Contains("Ativo"))
                            {
                                situacao = "Ativo";
                            }
                            else if(conteudo.Count() == 3 && lines[j].Contains("Inativo"))
                            {
                                situacao = "Inativo";
                            }
                        }
                        //Verifica se está lendo o cabeçalho da tabela
                        if (lines[j].Contains("Unidade de Saúde:"))
                        {
                            if (lines[j].Contains("-"))
                            {
                                nomeUnidade = "";
                                idUnidade = Convert.ToInt32(conteudo[3]);
                                for (int k = 5; k < conteudo.Length; k++)
                                {
                                    nomeUnidade = nomeUnidade + " " + conteudo[k];
                                }
                                //Cria uma nova unidade no banco
                                Unidade u = new Unidade();
                                u.ID_Unidade = idUnidade;
                                u.Descricao = nomeUnidade.Trim();
                                db.SP_InsertUnidade(u.ID_Unidade, u.Descricao);
                            }
                            else
                            {
                                nomeUnidade = "";
                                idUnidade = 0;
                                for (int k = 3; k < conteudo.Length; k++)
                                {
                                    nomeUnidade = nomeUnidade + " " + conteudo[k];
                                }
                                Unidade u = new Unidade();
                                u.ID_Unidade = idUnidade;
                                u.Descricao = nomeUnidade.Trim();
                                db.SP_InsertUnidade(u.ID_Unidade, u.Descricao);
                            }
                        }

                        // verifica se esta lendo um cliente
                        if (lines[j].Contains("Ano(s)") || lines[j].Contains("Mes(es)") || lines[j].Contains("Dia(s)"))
                        {
                            int coluna = 1;
                            string auxNomeCliente = "";
                            string auxEndereco = "";
                            Cliente cliente = new Cliente();
                            for (int k = 0; k < conteudo.Length; k++)
                            {
                                // coluna do ID
                                if (coluna == 1)
                                {
                                    cliente.ID_Cliente = Convert.ToInt32(conteudo[k]);
                                    coluna = 2;
                                }
                                // coluna do NOME
                                else if (coluna == 2)
                                {
                                    auxNomeCliente += conteudo[k] + " ";
                                    if (int.TryParse(conteudo[k + 1], out int tryResult))
                                    {
                                        cliente.Nome = auxNomeCliente.Trim();
                                        coluna = 3;
                                    }
                                }
                                // coluna da IDADE
                                else if (coluna == 3)
                                {
                                    cliente.Idade = Convert.ToInt32(conteudo[k]);
                                    coluna = 4;
                                }
                                // coluna do TIPO IDADE
                                else if (coluna == 4)
                                {
                                    cliente.TipoIdade = conteudo[k];
                                    coluna = 5;
                                }
                                // coluna do SEXO
                                else if (coluna == 5)
                                {
                                    cliente.Sexo = conteudo[k];
                                    coluna = 6;
                                    if (k == conteudo.Length-1)
                                    {
                                        cliente.CNS = "";
                                        cliente.Familia = "";
                                        cliente.Prontuario = "";
                                        cliente.Endereco = "";
                                        coluna = 10;
                                    }
                                    else if (Regex.IsMatch(conteudo[k + 1], @"[a-zA-Z]"))
                                    {
                                        cliente.CNS = "";
                                        cliente.Familia = "";
                                        cliente.Prontuario = "";
                                        coluna = 9;
                                    }
                                }
                                // coluna do CNS
                                else if (coluna == 6)
                                {
                                    cliente.CNS = conteudo[k];
                                    coluna = 7;
                                    if (k == conteudo.Length - 1)
                                    {
                                        cliente.Familia = "";
                                        cliente.Prontuario = "";
                                        cliente.Endereco = "";
                                        coluna = 10;
                                    }
                                    else if (Regex.IsMatch(conteudo[k + 1], @"[a-zA-Z]"))
                                    {
                                        cliente.Familia = "";
                                        cliente.Prontuario = "";
                                        coluna = 9;
                                    }
                                }
                                // coluna da FAMILIA
                                else if (coluna == 7)
                                {
                                    cliente.Familia = conteudo[k];
                                    coluna = 8;
                                    if (k == conteudo.Length - 1)
                                    {
                                        cliente.Prontuario = "";
                                        cliente.Endereco = "";
                                        coluna = 10;
                                    }
                                    else if (Regex.IsMatch(conteudo[k + 1], @"[a-zA-Z]"))
                                    {
                                        cliente.Prontuario = "";
                                        coluna = 9;
                                    }
                                }
                                // coluna do PRONTUARIO
                                else if (coluna == 8)
                                {
                                    cliente.Prontuario = conteudo[k];
                                    coluna = 9;
                                    if (k == conteudo.Length - 1)
                                    {
                                        cliente.Endereco = "";
                                        coluna = 10;
                                    }
                                }
                                // coluna do ENDERECO
                                else if (coluna == 9)
                                {
                                    auxEndereco += conteudo[k] + " ";
                                    if (k == conteudo.Length - 1)
                                    {
                                        cliente.Endereco = auxEndereco.Trim();
                                    }
                                }
                            }
                            cliente.ID_Unidade = idUnidade;
                            cliente.Situacao = situacao;
                            db.SP_InsertCliente(cliente.ID_Cliente, cliente.Nome, cliente.Idade, cliente.TipoIdade, cliente.Sexo, cliente.CNS, cliente.Familia, cliente.Prontuario, cliente.Endereco, cliente.Situacao, cliente.ID_Unidade);
                            contador++;
                        }
                    }
                }
            }   //final using reader
            //retorna o numero de clientes adicionados
            return contador;
        }
    }
}