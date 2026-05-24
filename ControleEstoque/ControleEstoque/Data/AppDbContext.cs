using ControleEstoque.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ControleEstoque.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Permissao> Permissoes { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }

        public DbSet<Venda> Vendas { get; set; }
        public DbSet<VendaItem> VendaItens { get; set; }
        public DbSet<ContasReceber> ContasReceber { get; set; }
        public DbSet<ContasPagar> ContasPagar { get; set; }
        public DbSet<CondicaoPagamento> CondicoesPagamento { get; set; }

        public DbSet<Orcamento> Orcamentos { get; set; }

        public DbSet<OrcamentoItem> OrcamentoItens { get; set; }

        public DbSet<EntradaEstoque> EntradasEstoque { get; set; }

        public DbSet<EntradaEstoqueItem> EntradaEstoqueItens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Permissao>().ToTable("permissoes");
            modelBuilder.Entity<Cliente>().ToTable("clientes");
            modelBuilder.Entity<Fornecedor>().ToTable("fornecedores");
            modelBuilder.Entity<Produto>().ToTable("produtos");
            modelBuilder.Entity<Compra>().ToTable("compras");
            modelBuilder.Entity<MovimentacaoEstoque>().ToTable("movimentacoes_estoque");
            modelBuilder.Entity<Venda>().ToTable("vendas");
            modelBuilder.Entity<VendaItem>().ToTable("venda_itens");
            modelBuilder.Entity<ContasReceber>().ToTable("contas_receber");
            modelBuilder.Entity<ContasPagar>().ToTable("contas_pagar");
            modelBuilder.Entity<CondicaoPagamento>().ToTable("condicoes_pagamento");
            modelBuilder.Entity<Orcamento>().ToTable("orcamentos");
            modelBuilder.Entity<OrcamentoItem>().ToTable("orcamento_itens");
            modelBuilder.Entity<EntradaEstoque>().ToTable("entradas_estoque");
            modelBuilder.Entity<EntradaEstoqueItem>().ToTable("entrada_estoque_itens");
        }
    }
}
