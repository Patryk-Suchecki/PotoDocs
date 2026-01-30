using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using System.Data;

namespace PotoDocs.API.Services;

public interface IInvoiceNumberGenerator
{
    Task<int> GetNextNumberAsync(DateTime date, InvoiceType type);
}

public class InvoiceNumberGenerator(PotodocsDbContext dbContext) : IInvoiceNumberGenerator
{
    private readonly PotodocsDbContext _dbContext = dbContext;

    public async Task<int> GetNextNumberAsync(DateTime date, InvoiceType type)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var sequence = await _dbContext.InvoiceSequences
                .FirstOrDefaultAsync(s => s.Year == date.Year && s.Month == date.Month && s.Type == type);

            if (sequence != null)
            {
                sequence.LastNumber++;
                _dbContext.Update(sequence);
            }
            else
            {

                sequence = new InvoiceSequence
                {
                    Year = date.Year,
                    Month = date.Month,
                    Type = type,
                    LastNumber = 1
                };

                await _dbContext.InvoiceSequences.AddAsync(sequence);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return sequence.LastNumber;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}