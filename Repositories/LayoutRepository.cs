using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TMSWeb.Models;
using TMSWeb.Models.Entities.LOG;
using Microsoft.EntityFrameworkCore;

namespace TMSWeb.Repositories;
public class LayoutRepository
{
    private DbavalogContext db;

    public LayoutRepository(DbavalogContext context)
    {
        db = context;
    }

    public async Task<List<ViewGroupPriviledgeLevel1>> GetPriviledgeLevel1ByGroup(string group_code)
    {
        var query = await db.ViewGroupPriviledgeLevel1s.Where(w => w.GroupCode == group_code).OrderBy(o => o.Level01Index).ToListAsync();
        return query;
    }

    public async Task<List<ViewGroupPriviledgeLevel2>> GetPriviledgeLevel2ByGroup(string group_code, string uplevel_code)
    {
        var query = await db.ViewGroupPriviledgeLevel2s.Where(w => w.GroupCode == group_code && w.Level01Code == uplevel_code).OrderBy(o => o.Level02Index).ToListAsync();
        return query;
    }

    public async Task<List<ViewGroupPriviledgeLevel3>> GetPriviledgeLevel3ByGroup(string group_code, string uplevel_code)
    {
        var query = await db.ViewGroupPriviledgeLevel3s.Where(w => w.GroupCode == group_code && w.Level02Code == uplevel_code).ToListAsync();
        return query;
    }

    /*public void SetTheme(string name, string username)
    {
        var data = db.SeAccounts.Find(username);
        data.WebTheme = name;
        db.Entry(data).State = EntityState.Modified;
        db.SaveChanges();
    }

    public int GetUnbillNumber()
    {
        var query = db.TblJobDetails.Where(w => w.IsBilling == false && w.IsEnabled == true).ToList();
        int result = query.Count();

        return result;
    }*/
}