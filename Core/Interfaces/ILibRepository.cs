using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Interfaces
{
    public interface ILibRepository
    {
        Task<List<LibraryData>> GetLibraryValues();

        Task<List<LibraryData>> GetResponsibleValues();
    }
}
