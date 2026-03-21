using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.Personas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    /// <summary>
    /// DataAccessLayer around Personas.
    /// </summary>
    public class PersonasDal : StorageDal, IPersonasDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public PersonasDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();

            Cleanup();
        }

        private void Cleanup()
        {
            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();

            int Personas = dbContext.Personas.Count();

            if (Personas > 0)
                return;

            dbContext.Personas.Add(new PersonaDbModel
            {
                PersonaId = Guid.NewGuid().ToString(),
                LastActivityAtUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow,
                IsDefault = true,
                Name = "Default Persona",
                Description = "",
            });

            dbContext.SaveChanges();
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        public async Task<PersonaDbModel[]> GetPersonasAsync()
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Personas.ToArray();
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("92725177-a5b1-4787-9beb-7d9f19a5691e", $"Error when querying Db on table Personas.", ex);
                return null;
            }
        }

        public async Task<PersonaDbModel> GetPersonaByIdAsync(string id)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Personas.FirstOrDefault(w => w.PersonaId == id);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("8e10523c-4795-4ca8-818d-d5e979b70000", $"Error when querying Db on table Personas.", ex);
                return null;
            }
        }

        public async Task<PersonaDbModel> AddPersonaAsync(AddPersonaQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert models
                PersonaDbModel personaDbModel = new PersonaDbModel
                {
                    PersonaId = Guid.NewGuid().ToString(),
                    CreatedAtUtc = DateTime.UtcNow,
                    LastActivityAtUtc = DateTime.UtcNow,
                    Name = queryModel.Name,
                    IsDefault = false,
                    Description = queryModel.Description,
                };

                EntityEntry<PersonaDbModel> result = await dbContext.Personas.AddAsync(personaDbModel);

                if (result.State != EntityState.Added)
                {
                    LoggingManager.LogToFile("a3e0e0f5-822f-4e26-a1b6-85611c0b4dbe", $"Error when querying Db on table Personas. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}].");
                    return null;
                }


                if (dbContext.Personas.Count() <= 0)
                {
                    personaDbModel.IsDefault = true;
                    await UpdatePersonaAsync(personaDbModel);
                }

                await dbContext.SaveChangesAsync();
                return result.Entity;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("f2e33fe2-71fb-41f8-be8c-5232100acfa5", $"Error when querying Db on table Personas.", ex);
                return null;
            }
        }

        public async Task<bool> UpdatePersonaAsync(PersonaDbModel personaDbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var persona = dbContext.Personas.FirstOrDefault(w => w.PersonaId == personaDbModel.PersonaId);

                if (persona == null)
                {
                    LoggingManager.LogToFile("8ead90a8-4312-4e0a-b733-afb690235afe", $"Persona [{personaDbModel.PersonaId}] to update wasn't found in storage.");
                    return false;
                }

                persona.LastActivityAtUtc = DateTime.UtcNow;

                // Only handle overridable fields
                persona.Description = personaDbModel.Description;
                persona.Name = personaDbModel.Name;
                persona.IsDefault = personaDbModel.IsDefault;

                if (persona.IsDefault)
                {
                    // Disable any other that is default currently
                    var personasToDisableDefault = dbContext.Personas.Where(w => w.PersonaId != personaDbModel.PersonaId && w.IsDefault).ToArray();

                    foreach (PersonaDbModel item in personasToDisableDefault)
                    {
                        item.IsDefault = false;
                    }
                } else
                {
                    // Make sure there's one default
                    var personasToDisableDefault = dbContext.Personas.Where(w => w.IsDefault).ToArray();

                    if (personasToDisableDefault.Length <= 0)
                    {
                        var personaToEnableDefault = dbContext.Personas.Where(w => w.PersonaId != persona.PersonaId).OrderBy(w => w.LastActivityAtUtc).FirstOrDefault();

                        if (personaToEnableDefault != null)
                        {
                            personaToEnableDefault.IsDefault = true;
                        } else
                        {
                            // There's no default persona and there's only one persona in db, so it is what it is
                            persona.IsDefault = true;
                        }
                    }
                }

                EntityEntry<PersonaDbModel> result = dbContext.Personas.Update(persona);
                if (result.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("16b64607-90d7-4d18-be09-6f70f38509e7", $"Error when updating Persona. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(personaDbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("b2ddbb75-5d2b-441f-afe2-cd96bce8fe6c", $"Error when querying pending queries on table Personas.", ex);
                return false;
            }
        }

        public async Task<bool> DeletePersonaAsync(PersonaDbModel personaDbModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                var persona = dbContext.Personas.AsNoTracking().FirstOrDefault(w => w.PersonaId == personaDbModel.PersonaId);

                if (persona == null)
                {
                    LoggingManager.LogToFile("b94bfcb9-6390-40c7-a584-72ff9f55e9de", $"Persona [{personaDbModel.PersonaId}] to update wasn't found in storage.");
                    return false;
                }

                EntityEntry<PersonaDbModel> result = dbContext.Personas.Remove(personaDbModel);
                if (result.State != EntityState.Deleted)
                {
                    LoggingManager.LogToFile("a72236f5-5a06-43e8-9bc3-fc5c04a9e472", $"Error when deleting a Persona. State was [{result.State}]. Result: [{JsonCommonSerializer.SerializeToString(result)}]. dbModel: [{JsonCommonSerializer.SerializeToString(personaDbModel)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("15a4f72f-1298-4aa2-9c9e-09a488f1353d", $"Error when querying pending queries on table Personas.", ex);
                return false;
            }
        }
    }
}
