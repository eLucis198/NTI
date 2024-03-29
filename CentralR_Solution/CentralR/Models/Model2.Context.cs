﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CentralR.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class CentralREntities2 : DbContext
    {
        public CentralREntities2()
            : base("name=CentralREntities2")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Cliente> Cliente { get; set; }
        public virtual DbSet<Unidade> Unidade { get; set; }
        public virtual DbSet<AgendamentoConsulta> AgendamentoConsulta { get; set; }
        public virtual DbSet<ViewAgendamentoConsulta> ViewAgendamentoConsulta { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
    
        public virtual int SP_InsertCliente(Nullable<int> idCliente, string nome, Nullable<int> idade, string tipoIdade, string sexo, string cns, string familia, string prontuario, string endereco, string situacao, Nullable<int> idUnidade)
        {
            var idClienteParameter = idCliente.HasValue ?
                new ObjectParameter("idCliente", idCliente) :
                new ObjectParameter("idCliente", typeof(int));
    
            var nomeParameter = nome != null ?
                new ObjectParameter("nome", nome) :
                new ObjectParameter("nome", typeof(string));
    
            var idadeParameter = idade.HasValue ?
                new ObjectParameter("idade", idade) :
                new ObjectParameter("idade", typeof(int));
    
            var tipoIdadeParameter = tipoIdade != null ?
                new ObjectParameter("tipoIdade", tipoIdade) :
                new ObjectParameter("tipoIdade", typeof(string));
    
            var sexoParameter = sexo != null ?
                new ObjectParameter("sexo", sexo) :
                new ObjectParameter("sexo", typeof(string));
    
            var cnsParameter = cns != null ?
                new ObjectParameter("cns", cns) :
                new ObjectParameter("cns", typeof(string));
    
            var familiaParameter = familia != null ?
                new ObjectParameter("familia", familia) :
                new ObjectParameter("familia", typeof(string));
    
            var prontuarioParameter = prontuario != null ?
                new ObjectParameter("prontuario", prontuario) :
                new ObjectParameter("prontuario", typeof(string));
    
            var enderecoParameter = endereco != null ?
                new ObjectParameter("endereco", endereco) :
                new ObjectParameter("endereco", typeof(string));
    
            var situacaoParameter = situacao != null ?
                new ObjectParameter("situacao", situacao) :
                new ObjectParameter("situacao", typeof(string));
    
            var idUnidadeParameter = idUnidade.HasValue ?
                new ObjectParameter("idUnidade", idUnidade) :
                new ObjectParameter("idUnidade", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SP_InsertCliente", idClienteParameter, nomeParameter, idadeParameter, tipoIdadeParameter, sexoParameter, cnsParameter, familiaParameter, prontuarioParameter, enderecoParameter, situacaoParameter, idUnidadeParameter);
        }
    
        public virtual int SP_InsertUnidade(Nullable<int> idUnidade, string descricao)
        {
            var idUnidadeParameter = idUnidade.HasValue ?
                new ObjectParameter("idUnidade", idUnidade) :
                new ObjectParameter("idUnidade", typeof(int));
    
            var descricaoParameter = descricao != null ?
                new ObjectParameter("descricao", descricao) :
                new ObjectParameter("descricao", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SP_InsertUnidade", idUnidadeParameter, descricaoParameter);
        }
    }
}
