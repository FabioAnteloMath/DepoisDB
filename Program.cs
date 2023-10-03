using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMySql<ApplicationDbContext>(
    builder.Configuration["Database:MySql"],
    new MySqlServerVersion(new Version(8, 0, 21)),
    mySqlOptions =>
    { });
var app = builder.Build();

app.MapPost("/patient", (PatientRequest patientRequest, ApplicationDbContext context) =>
{
    var patient = new Patient
    {
        Cid = patientRequest.Cid,
        Name = patientRequest.Name,
        Description = patientRequest.Description,
    };
    context.Patients.Add(patient);
    context.SaveChanges();
    return Results.Created($"/patients/{patient.Id}", patient.Id);
});

app.MapGet("/patient/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
    var patient = context.Patients.Where(p => p.Id == id).FirstOrDefault();
    if (patient != null)
    {
        return Results.Ok(patient);
    }
    return Results.NotFound();
});

app.MapPut("/patient/{id}",([FromRoute] int id, PatientRequest patientRequest, ApplicationDbContext context) =>
{
    var patient = context.Patients.Where(p => p.Id == id).First();
    
    patient.Name = patientRequest.Name;
    patient.Cid = patientRequest.Cid;
    patient.Description = patientRequest.Description;

    context.SaveChanges();
    return Results.Ok();
});

app.MapDelete("/patient/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
    var product = context.Patients.Where(p => p.Id == id).First();
    context.Patients.Remove(product);
    context.SaveChanges();
    return Results.Ok();
});

app.Run();

public record PatientRequest(string Cid, string Name, string Description);


public class Patient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Cid { get; set; }
    public string Description { get; set; }
}

public static class PatientList
{
    public static List<Patient> Patients { get; set; }
    public static void Add(Patient patient)
    {
        if (Patients == null)
            Patients = new List<Patient>();

        Patients.Add(patient);
    }

    public static Patient GetBy(String cid)
    {
        return Patients.First(p => p.Cid == cid);
    }

    internal static void Remove(Patient patientSaved)
    {
        throw new NotImplementedException();
    }
}


public class ApplicationDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Patient>().Property(p => p.Description).HasMaxLength(500).IsRequired(false);
        builder.Entity<Patient>().Property(p => p.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Patient>().Property(p => p.Cid).HasMaxLength(20).IsRequired();
    }
}