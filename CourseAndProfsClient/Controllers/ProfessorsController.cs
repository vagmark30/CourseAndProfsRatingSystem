﻿namespace CourseAndProfsClient.Controllers
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using CourseAndProfsPersistence;
  using CourseAndProfsPersistence.Helpers;
  using CourseAndProfsPersistence.Models;
  using Helpers;
  using CourseAndProfsClientModels;
  using CourseAndProfsClientModels.Dto;

  using Kritikos.PureMap.Contracts;

  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;

  [Route("api/professor")]
  [ApiController]
  public class ProfessorsController : BaseController<ProfessorsController>
  {
    public ProfessorsController(ILogger<ProfessorsController> logger, CaPDbContext ctx, IPureMapper mapper)
      : base(logger, ctx, mapper)
    {
    }

    /// <summary>
    /// Returns all professors. You can pass parameters to handle page and result count.
    /// </summary>
    /// <param name="itemsPerPage">Define how many items shall be returned. </param>
    /// <param name="page">Choose which page of the results shall be returned.</param>
    /// <returns>Returns a list of Professors.</returns>
    [HttpGet("")]
    public async Task<ActionResult<List<ProfessorDto>>> GetProfessors(int itemsPerPage = 20, int page = 1)
    {
      var toSkip = itemsPerPage * (page - 1);

      var professorsQuery = Context.Professors
        .TagWith("Retrieving all professors")
        .OrderBy(x => x.Id);

      var totalProfessors = await professorsQuery.CountAsync();

      if (page > ((totalProfessors / itemsPerPage) + 1))
      {
        return BadRequest("Page doesn't exist");
      }

      var pagedProfessors = await professorsQuery
        .Skip(toSkip)
        .Take(itemsPerPage)
        .ToListAsync();

      var result = new PagedResult<ProfessorDto>
      {
        Results = pagedProfessors.Select(x => Mapper.Map<Professor, ProfessorDto>(x)).ToList(),
        Page = page,
        TotalPages = (totalProfessors / itemsPerPage) + 1,
        TotalElements = totalProfessors,
      };

      return Ok(result);
    }

    /// <summary>
    /// Returns an professor provided an ID.
    /// </summary>
    /// <param name="id">Professor's ID.</param>
    /// <returns>One single Professor.</returns>
    /// <response code="400">Professor was not found.</response>
    [HttpGet("{id}")]
    public ActionResult<ProfessorDto> GetProfessor(long id)
    {
      var professor = Context.Professors.SingleOrDefault(x => x.Id == id);

      if (professor == null)
      {
        Logger.LogWarning(LogTemplates.NotFound, nameof(Professor), id);
        return NotFound($"No {nameof(Professor)} with Id {id} found in database");
      }

      Logger.LogInformation(LogTemplates.RequestEntity, nameof(Professor), id);

      return Ok(Mapper.Map<Professor, ProfessorDto>(professor));
    }

    /// <summary>
    /// Adds an professor provided the necessary information.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("")]
    public async Task<ActionResult<ProfessorDto>> AddProfessor([FromBody] AddProfessorDto dto)
    {
      var professor = Mapper.Map<AddProfessorDto, Professor>(dto);

      Context.Professors.Add(professor);

      await Context.SaveChangesAsync();
      Logger.LogInformation(LogTemplates.CreatedEntity, nameof(Professor), professor);

      return CreatedAtAction(nameof(GetProfessor), new { id = professor.Id }, Mapper.Map<Professor, ProfessorDto>(professor));
    }

    /// <summary>
    /// We delete a user provided an ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    public async Task<ActionResult> DeleteProfessor(int id)
    {
      var professor = Context.Professors.SingleOrDefault(x => x.Id == id);

      if (professor == null)
      {
        Logger.LogWarning(LogTemplates.NotFound, nameof(Professor), id);
        return NotFound("No professor found in the database");
      }

      Context.Professors.Remove(professor);

      await Context.SaveChangesAsync();
      Logger.LogInformation(LogTemplates.Deleted, nameof(Professor), id);

      return NoContent();
    }

    /// <summary>
    /// We update an Professor provided all the necessary information. Id is required.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProfessorDto>> UpdateProfessor(int id, AddProfessorDto dto)
    {
      var professor = Context.Professors.SingleOrDefault(x => x.Id == id);

      if (professor == null)
      {
        Logger.LogWarning(LogTemplates.NotFound, nameof(Professor), id);
        return NotFound($"No {nameof(Professor)} with Id {id} found in database");
      }

      professor.FullName = dto.FullName;
      professor.Mail = dto.Mail;
      professor.Phone = dto.Phone;
      professor.Office = dto.Office;
      professor.EOffice = dto.EOffice;

      await Context.SaveChangesAsync();
      Logger.LogInformation(LogTemplates.Updated, nameof(Professor), professor);

      return Ok(Mapper.Map<Professor, ProfessorDto>(professor));
    }
  }
}
