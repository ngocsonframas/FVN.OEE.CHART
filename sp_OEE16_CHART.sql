USE [OEE]
GO
/****** Object:  StoredProcedure [dbo].[sp_OEE16_CHART]    Script Date: 12/26/2019 2:24:20 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
                                  
-- oee16 "2019-07-01","2019-07-30",'0',"nqhuy"                                      
                                
ALTER PROCEDURE [dbo].[sp_OEE16_CHART]                                      
                                
(                                      
                                
 @V_FROMDATE datetime,                                      
                                
 @V_TODATE datetime,                                      
                                
 @V_CATEGORYID VARCHAR(20),                                                                                   
                                
    @V_USERID  NVARCHAR(256)                                      
                                
)                                      
                                
                                    
AS                                      
                                
BEGIN                                      
 SET NOCOUNT ON;               
  
       
 Declare @CATEGORYID1 nvarchar(10)                                          
                                    
 Declare @CATEGORYID2 nvarchar(10)                                          
                                
 Declare @CATEGORYID3 nvarchar(10)                                          
                                
 Declare @CATEGORYID4 nvarchar(10)                                          
                                
  Declare @CATEGORYID5 nvarchar(10)                                          
                                
 Declare @CATEGORYID6 nvarchar(10)                                          
                                
 Declare @CATEGORYID7 nvarchar(10)                                           
                                
 Declare @cate as varchar(2)                                     
  Declare @cateR as varchar(2)                                    
                                    
if @V_CATEGORYID = 'HOI'                                           
                                
begin                                          
                                
set @CATEGORYID1 = '001'                                          
                                
set @CATEGORYID2 = '003'                                          
                                
set @CATEGORYID3 = '005'                                          
                                
set @CATEGORYID4 = '000'                                          
                                
set @CATEGORYID5 = '000'                                          
                                
set @CATEGORYID6 = '000'                                          
                                
set @CATEGORYID7 = '000'                                          
                                
set  @cate = 'TI'                                  
set  @cateR = 'PA'                                  
                                    
end                                          
                                    
else                                          
if @V_CATEGORYID = 'HO'                                  
                                
begin                                          
      
set @CATEGORYID1 = '001'                                          
                                
set @CATEGORYID2 = '003'                                          
                                
set @CATEGORYID3 = '000'                                          
                                
set @CATEGORYID4 = '000'                                         
                                
set @CATEGORYID5 = '000'                                          
                                
set @CATEGORYID6 = '000'                                          
                                
set @CATEGORYID7 = '000'                                          
                                
set  @cate = 'TI'                                  
set  @cateR = 'PA'                                  
                                    
end                                          
                                
else                                          
                                
if @V_CATEGORYID = 'HI'                                           
                                
begin                                          
                                    
set @CATEGORYID1 = '001'                                          
                                
set @CATEGORYID2 = '000'                                  
                                
set @CATEGORYID3 = '005'                                          
                                
set @CATEGORYID4 = '000'                                          
                           
set @CATEGORYID5 = '000'                                          
                                
set @CATEGORYID6 = '000'                                          
                                
set @CATEGORYID7 = '000'                                      
                                
set  @cate = 'TI'                                  
set  @cateR = 'PA'                                  
                                    
end                                          
                                
else                                    
                                
if @V_CATEGORYID = '0'                                           
                                
begin                                          
                                
set @CATEGORYID1 = '001'                                          
                 
set @CATEGORYID2 = '003'                                          
                                
set @CATEGORYID3 = '005'                                          
                                
set @CATEGORYID4 = '006'                                          
                                
set @CATEGORYID5 = '000'                          
                                
set @CATEGORYID6 = '000'                                          
                                
set @CATEGORYID7 = '000'                                          
                                
set  @cate = 'TI'                                  
set  @cateR = 'PA'                                   
                                    
end                       
                                
  else                                          
                                
if @V_CATEGORYID = '009'                                           
                                
begin                                          
                                
set @CATEGORYID1 = '000'                                          
                                
set @CATEGORYID2 = '000'                                          
                                
set @CATEGORYID3 = '000'                                          
                                
set @CATEGORYID4 = '000'                                          
                                
set @CATEGORYID5 = '000'                                          
            
set @CATEGORYID6 = '000'                                          
                                
set @CATEGORYID7 = '009'                                          
                                
set  @cate = 'TI'                                  
set  @cateR = 'PA'                  
                                    
end                                          
                                
else                                          
                                
begin                                          
                                    
set @CATEGORYID1 = @V_CATEGORYID                                          
                                
set @CATEGORYID2 = @V_CATEGORYID                                          
                                
set @CATEGORYID3 = @V_CATEGORYID                                          
                                
set @CATEGORYID4 = @V_CATEGORYID                                          
                                
set @CATEGORYID5 = @V_CATEGORYID                                          
                                
set @CATEGORYID6 = @V_CATEGORYID                                          
                                
set @CATEGORYID7 = @V_CATEGORYID                                          
                                
set  @cate = 'TI'                                  
set  @cateR = 'PA'                                  
                                   
end                                          
                   
 DELETE FROM tmpTLBD;                                      
                                
 DECLARE @ParmDefinition nvarchar(500);                                      
                                
 SET @ParmDefinition = N'@OEEType VARCHAR(2), @PA VARCHAR(2),@FROMDATE DATETIME, @TODATE DATETIME,@CATEGORYID1V VARCHAR(20),@CATEGORYID2V VARCHAR(20),@CATEGORYID3V VARCHAR(20),@CATEGORYID4V VARCHAR(20),@CATEGORYID5V VARCHAR(20),@CATEGORYID6V VARCHAR(20),
  
    
@CATEGORYID7V VARCHAR(20),@Extr varchar(3),@V_CATEGORYID VARCHAR(20)';                                      
                                
 DECLARE @CList VARCHAR(2000), @CList0 VARCHAR(2000), @CList1 VARCHAR(2000), @CList2 VARCHAR(2000)                                      
                                
 DECLARE @SQLString  NVARCHAR(4000)                         
                                
 DECLARE @result TABLE ( IID varchar(20),CID varchar(3),C varchar(4),CVal float,PPS int)                                      
                                
 DEClare @Emp Table(OEEID varchar(50),IID varchar(20),ShiftID tinyint,MID varchar(4),OEEDate datetime,Opr varchar(4),Trim varchar(4),QC varchar(4),PPS int,CID varchar(20))                                      
                                
DEClare @Emp1 Table(OEEID varchar(50),IID varchar(20),ShiftID tinyint,MID varchar(4),OEEDate datetime,Opr varchar(4),Trim varchar(4),QC varchar(4),times int,PPS int,CID varchar(20))                                      
                                
 DEClare @Emp2 Table(OEEID varchar(50),IID varchar(20),ShiftID tinyint,MID varchar(4),OEEDate datetime,Opr varchar(4),Trim varchar(4),QC varchar(4),times int,PPS int,CID varchar(20))                                      
                                
 DEClare @Emp3 Table(OEEID varchar(50),IID varchar(20),ShiftID tinyint,MID varchar(4),OEEDate datetime,Opr varchar(4),Trim varchar(4),QC varchar(4),times int,PPS int,CID varchar(20))                                      
                                
 DEClare @Emp4 Table(OEEID varchar(50),IID varchar(20),ShiftID tinyint,MID varchar(4),OEEDate datetime,NOpr float,NTrim float,NQC float,times int,PPS int,CID varchar(20))                                      
                                
 DEClare @Emp5 Table(OEEID varchar(50),IID varchar(20),ShiftID tinyint,MID varchar(4),OEEDate datetime,Opr float,Trim float,QC float,TOpr float,TTrim float, TQC float,PPS int,CID varchar(20))    
                           
 DEClare @EmpTT Table(OEEID varchar(50),IID varchar(20),ShiftID tinyint,MID varchar(4),OEEDate datetime,Opr float,Trim float,QC float,TOpr float,TTrim float, TQC float,PPS int,CID varchar(20))                                                 
                                
  if @V_CATEGORYID = '009'                                      
         
 begin                                       
                                
 SELECT  @CList = STUFF(( SELECT DISTINCT                                      
         ' ,A.' + ID                                      
       FROM    [tblOEETemplate] WITH (NOLOCK)                                     
                                
   WHERE ISNULL(DTGroup,'')<>'' AND Kind = 3 AND [Level] = 1 and DTGroup = 'MT' and CAST(right(ID,3) as int) < 166                                      
                                
       ORDER BY ' ,A.' + ID                                      
                                
       FOR XML PATH('')                                  
                                
          ), 1, 2, '') + ''                                      
                                
 SELECT @CList0 =  STUFF(( SELECT DISTINCT                                      
                                
         ' ,' + ID                                      
                                
       FROM     [tblOEETemplate]WITH (NOLOCK)                                      
                                
       WHERE ISNULL(DTGroup,'')<>'' AND Kind = 3 AND [Level] = 1 and DTGroup = 'MT' and CAST(right(ID,3) as int) < 166                                      
                                
       ORDER BY ' ,' + ID                                      
                                
       FOR XML PATH('')                                      
                                
          ), 1, 2, '') + ''                                      
                                
 end                                      
                                    
 else                                       
                                
 begin                                      
                                
 SELECT  @CList = STUFF(( SELECT DISTINCT                                      
                                
         ' ,A.' + ID                                      
                                
       FROM    TBLOEETEMPLATE WITH (NOLOCK)                                      
                                
                                    
       WHERE ISNULL(DTGroup,'')<>'' AND Kind = 3 AND [Level] = 1 and DTGroup = 'MT' and CAST(right(ID,3) as int) < 166                                      
                                
       ORDER BY ' ,A.' + ID                                      
                                
       FOR XML PATH('')                  
                                
          ), 1, 2, '') + ''                                      
                                
 SELECT @CList0 =  STUFF(( SELECT DISTINCT                                      
                 ' ,' + ID                                      
                                
       FROM    TBLOEETEMPLATE   WITH (NOLOCK)                                   
                                
       WHERE ISNULL(DTGroup,'')<>'' AND Kind = 3 AND [Level] = 1 and DTGroup = 'MT' and CAST(right(ID,3) as int) < 166                                      
                                
       ORDER BY ' ,' + ID                                      
                                
       FOR XML PATH('')                                      
                                
          ), 1, 2, '') + ''                                      
                                
 end                                      
                                    
 SET @SQLString ='CREATE TABLE #tmpOEE16_Detail(IID NVARCHAR(20),CID varchar(3),[C] [varchar](4) NOT NULL,[CVal] [float] NULL,PPS int);' +                                      
                                
     'INSERT INTO #tmpOEE16_Detail(IID,CID,C, CVal,PPS) ' +                                      
                                
     ' SELECT IID,CID,C, SUM(CAST(CVAL AS FLOAT)) AS CVal,PPS FROM (SELECT IID,CID,OEEID,PPS, ' + @CList +                                       
                                
     ' FROM TBLOEEDETAIL  A  WITH (NOLOCK)                                    
                                
      INNER JOIN TBLOEEHEADER B WITH (NOLOCK) ON B.OEEID = A.F000                                       
                                
  INNER JOIN (SELECT DISTINCT F000 FROM TBLITEM WITH (NOLOCK) WHERE F004 not in (@Extr,008) and  (F004  in (select Id from dbo.Get_List_Category('''',@V_CATEGORYID)) )) I ON I.F000 = B.IID                                      
                                
  WHERE OEETYPE = @OEEType AND DATEDIFF(MINUTE, OEEDATE, CONVERT(DATETIME, @FROMDATE))<= 0 AND DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME, @TODATE))>=0  ) p                                      
                                
      UNPIVOT (CVal FOR C IN                                      
                                
      ('+ @CList0 + '))AS unpvt GROUP BY C,IID,CID,PPS order by Cval desc;select * from #tmpOEE16_Detail where cval>0 ;'                                      
                                
      print @SQLString                          
                                
insert @result                                            
                                
EXEC sp_executesql @SQLString, @ParmDefinition, @OEEType = @cate,@PA = 'PA', @FROMDATE = @V_FROMDATE, @TODATE = @V_TODATE,@CATEGORYID1V = @CATEGORYID1,                                      
                                    
@CATEGORYID2V = @CATEGORYID2,                                      
                                    
@CATEGORYID3V = @CATEGORYID3,                                      
                                    
@CATEGORYID4V = @CATEGORYID4,                                      
                                    
@CATEGORYID5V = @CATEGORYID5,                                      
                                    
@CATEGORYID6V = @CATEGORYID6,                                      
                                    
@CATEGORYID7V = @CATEGORYID7,@Extr = '007' ,@V_CATEGORYID=@V_CATEGORYID                                            
                                
 insert @result(IID,C,CVal,PPS,CID)                                      
                                
 select A.IID,B.DTID as C, sum(B.TI) as CVal,A.PPS,A.CID                                      
                                
 from (select * from tblOEEHeader WITH (NOLOCK)) A                                       
                                
 left join (SELECT DISTINCT F000,F001 FROM TBLITEM where ((F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID))) )  ) I ON I.F000 = A.IID                                       
                                
 left outer join (select tblOEEDTProcess.* from tblOEEDTProcess WITH (NOLOCK) left join tbloeetemplate WITH (NOLOCK) on tbloeetemplate.ID = DTID where  LEFT(ProcessID,1)<>'z' and DTGroup = 'MT' ) B on A.OEEID = B.OEEID                                      
                  
 WHERE  DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME, @v_FROMDATE))<= 0 AND DATEDIFF(MINUTE,OEEDATE,CONVERT(DATETIME, @v_TODATE))>=0                                       
                                
 group by B.DTID,A.IID,A.PPS,A.CID                                       
                          
  select IID, sum(CVal) MT,PPS,CID into #tmpOEE16_1_Detail from @result group by IID,PPS,CID order by iid;                                                                       
                                
insert  @Emp(OEEID,IID,ShiftID,MID,OEEDate,Opr,Trim,QC,PPS,CID)                                      
                
select OEEID,IID,ShiftID,MID,OEEDate, Opr1,Trim,QC,PPS,CID from tblOEEHeader  h WITH (NOLOCK) inner join tblitem i on h.IID = i.F000                        
where OEEDate between @V_FROMDATE and @V_TODATE and (i.F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))                                      
                        
union                                       
                                
select OEEID,IID,ShiftID,MID,OEEDate, Opr2,Trim2,QC2,PPS,CID from tblOEEHeader h WITH (NOLOCK) inner join tblitem i on h.IID = i.F000                         
where OEEDate between @V_FROMDATE and @V_TODATE and (i.F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))                                      
                                
union                                       
                                
select OEEID,IID,ShiftID,MID,OEEDate, Opr3,Trim3, null,PPS,CID from tblOEEHeader h WITH (NOLOCK) inner join tblitem i on h.IID = i.F000                         
where OEEDate between @V_FROMDATE and @V_TODATE and (i.F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))                                        
                                
union                                       
                                
select OEEID,IID,ShiftID,MID,OEEDate, Opr4,Trim4, null,PPS,CID from tblOEEHeader h WITH (NOLOCK) inner join tblitem i on h.IID = i.F000                         
where OEEDate between @V_FROMDATE and @V_TODATE and (i.F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))                                        
                                
union                                       
                                
select OEEID,IID,ShiftID,MID,OEEDate, Opr5,null, null,PPS,CID from tblOEEHeader h WITH (NOLOCK) inner join tblitem i on h.IID = i.F000                     
where OEEDate between @V_FROMDATE and @V_TODATE and (i.F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))                                       
                                    
 CREATE TABLE #tmpOEE21_Detail      
                                
 (                                     
                                
  OEEID  VARCHAR(50),                                      
  OEEDATE  DATETIME,                                      
  IID   VARCHAR(20),                                                                           
  MODEID  TINYINT,                                      
  MID   VARCHAR(4),                                                                            
  CID   VARCHAR(3),                                      
  O   FLOAT,                                      
  A   FLOAT,                                      
  P   FLOAT,                                      
  Q   FLOAT,                                      
  TSH   FLOAT,                                      
  ASH   FLOAT,                                      
  PPT   FLOAT,                                      
  DTL   FLOAT,                                      
  QCA   FLOAT,                                      
  QCG   FLOAT,                                      
  FTQ   FLOAT,                                      
  QCD   FLOAT,                                      
  COQ   FLOAT,                                      
  OT   FLOAT,                                      
  [NOT]  FLOAT,                                      
  FPT   FLOAT,                                      
  O1   Float,                                      
  TPM   Float,                                      
  Opr1  VARCHAR(20),                                
  Opr2  VARCHAR(20),                                      
  Opr3  VARCHAR(20),                                      
  Trim  VARCHAR(20),                                      
  Trim2  VARCHAR(20),                                      
  QC   VARCHAR(20),                                      
  QC2   VARCHAR(20),                                      
  ProdTL  VARCHAR(20),                                      
  ProdTL2  VARCHAR(20),                                      
  TrimTL  VARCHAR(20),                                    
  TrimTL2  VARCHAR(20),                                      
  QCTL  VARCHAR(20),                                    
  Comp nvarchar(50),                                    
  PPS int                                    
 )                                      
                                
 INSERT INTO #tmpOEE21_Detail(OEEID, OEEDATE,IID,  MODEID,MID,CID,                                      
      O, A, P, Q, TSH, ASH, PPT, DTL, QCA, QCG, FTQ, QCD, COQ,                                      
      OT, [NOT], FPT,O1,TPM,                                      
      Opr1, Opr2,Opr3, Trim,Trim2, QC,QC2, ProdTL,ProdTL2, TrimTL,TrimTL2, QCTL,Comp,PPS                                        
)                                      
                                
 SELECT OEE.OEEID, OEE.OEEDATE,OEE.IID, OEE.MODEID,OEE.MID,OEE.CID,                                        
   OEE.O*100 AS O, OEE.A*100 AS A, OEE.P*100 AS P, OEE.Q*100 AS Q,                                       
   OEE.TSH, OEE.ASH, OEE.PPT, OEE.DTL, OEE.QCA, OEE.QCG, OEE.FTQ*100, OEE.QCD, OEE.COQ,                                      
   OEE.OT, OEE.[NOT], OEE.FPT, OEE.O1, OEE.TPM,                                      
   OEE.Opr1, OEE.Opr2,OEE.Opr3, OEE.Trim,OEE.Trim2, OEE.QC,OEE.QC2, OEE.ProdTL,OEE.ProdTL2, OEE.TrimTL,OEE.TrimTL2, OEE.QCTL,Comp,PPS                                        
                                
 FROM                                      
 (                                      
  SELECT L.OEEID, L.OEEDATE, L.IID, L.MODEID,L.MID,L.CID,                                      
    L.O, L.A, L.P, L.Q, S.TSH, S.ASH, L.PPT, L.DTL, R.QCA, R.QCG, R.FTQ, R.QCD, L.COQ,                                      
    L.OT, L.[NOT], L.FPT,L.O1, L.TPM,                      
    L.Opr1, L.Opr2,L.Opr3, L.Trim,L.Trim2, L.QC,L.QC2, L.ProdTL, L.ProdTL2,L.TrimTL,L.TrimTL2, L.QCTL,L.Comp,L.PPS                                      
  FROM                                      
   (                                      
    SELECT B.OEEID, B.OEEDATE, B.IID,B.MODEID,B.MID,B.CID,                                      
      isnull((CAST(A.A901 AS FLOAT)),0) + isnull((CAST(AV.A901 as Float)),0) AS A,                                       
      CAST(A.P901 AS FLOAT) AS P,                                      
      CAST(A.Q901 AS FLOAT) AS Q,                                      
      CAST(A.L301 AS FLOAT) AS PPT,                                      
      isnull((CAST(A.A301 AS FLOAT)),0)+isnull((CAST(DT.A301 as Float)),0) AS DTL,--                                      
      CAST(A.COQ AS FLOAT) AS COQ,                                      
      CAST(A.OEEE AS FLOAT) AS O,                                      
      isnull((CAST(A.A401 AS FLOAT)),0)+ isnull((CAST(OT.A401 as Float)),0) AS OT, --                                
      CAST(A.P401 AS FLOAT) AS [NOT],                                      
      CAST(A.Q401 AS FLOAT) AS FPT,                                      
      Cast(A.L105 as float) as O1,                                      
      cast(A.L103 as float) as TPM,                                      
      B.Opr1, B.Opr2,B.Opr3, B.Trim,B.Trim2, B.QC,B.QC2, B.ProdTL,B.ProdTL2, B.TrimTL,B.TrimTL2, B.QCTL,cmp.F001 as Comp,PPS                               
    FROM (SELECT * FROM TBLOEEDETAIL WITH (NOLOCK) WHERE OEETYPE = 'TI') A INNER JOIN (SELECT * FROM TBLOEEHEADER WITH (NOLOCK) inner join tblitem on iid = f000                                       
    WHERE DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME, @v_FROMDATE)) <=0 AND DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME, @v_TODATE)) >=0 and (f004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))) B ON B.OEEID = A.F000                                      
    left JOIN (select TI as A401,OEEID from tblOEEDTProcess WITH (NOLOCK) where ProcessID = 'z4')OT on OT.OEEID = B.OEEID                                      
    left JOIN (select TI as A301,OEEID from tblOEEDTProcess WITH (NOLOCK) where ProcessID = 'z3')DT on DT.OEEID = B.OEEID                                    
    left JOIN (select TI as A901,OEEID from tblOEEDTProcess WITH (NOLOCK) where ProcessID = 'z9')AV on AV.OEEID = B.OEEID                              
 left join tblComp cmp on B.CID = cmp.F000                                      
   ) L,                                      
   (                            
    SELECT B.OEEID,                                      
      ((CAST(A.Q201 AS FLOAT)) + (CAST(A.Q202 AS FLOAT))) AS QCA,                                      
      (CAST(A.Q202 AS FLOAT)) AS QCG,                                      
      ((CAST(A.Q202 AS FLOAT))/dbo.ISZERO((CAST(A.Q201 AS FLOAT)) + (CAST(A.Q202 AS FLOAT)),1)) AS FTQ,                                      
      (CAST(A.Q201 AS FLOAT)) AS QCD,cmp.F001 as Comp,PPS                                         
    FROM (SELECT * FROM TBLOEEDETAIL WITH (NOLOCK) WHERE OEETYPE = @cateR) A INNER JOIN (SELECT * FROM TBLOEEHEADER WITH (NOLOCK) inner join tblitem on iid = f000                                       
    WHERE DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME, @v_FROMDATE)) <=0 AND DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME, @v_TODATE)) >=0 and (f004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))) B ON B.OEEID = A.F000                                      
     left join tblComp cmp on B.CID = cmp.F000                                
   ) R,                        (                                      
    SELECT B.OEEID,                                      
      isnull((CAST(A.L301 AS FLOAT)),0)+ isnull((CAST(OT.A401 as Float)),0) AS TSH, -- Target shot                                      
      CAST(A.P401 AS FLOAT) AS ASH,cmp.F001 as Comp,PPS    -- Actual shot                                      
    FROM (SELECT * FROM TBLOEEDETAIL WITH (NOLOCK) WHERE OEETYPE = 'SH') A INNER JOIN (SELECT * FROM TBLOEEHEADER WITH (NOLOCK) inner join tblitem on iid = f000                                       
    WHERE DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME,  @v_FROMDATE)) <=0 AND DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME,  @v_TODATE)) >=0 and (f004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)) )) B ON B.OEEID = A.F000                                      
    left JOIN (select SH as A401,OEEID from tblOEEDTProcess WITH (NOLOCK) where ProcessID = 'z4')OT on OT.OEEID = B.OEEID                            
    left join tblComp cmp on B.CID = cmp.F000                                         
   ) S                              
  WHERE L.OEEID = R.OEEID AND S.OEEID = R.OEEID                                      
 ) OEE                                      
                                
                             
                                
 CREATE TABLE #tmpOEE21_1_Detail                                      
                                
 (                                      
  OEEID  VARCHAR(50),                                      
  OEEDATE  DATETIME,                                      
  IID   VARCHAR(20),                                      
  MODEID  TINYINT,                                      
  MID   VARCHAR(4),                        
  CID   VARCHAR(3),                                      
  O   FLOAT,                                      
  A   FLOAT,                                      
  P   FLOAT,                                      
  Q   FLOAT,                                      
  TSH   FLOAT,         
  ASH   FLOAT,                                      
  PPT   FLOAT,                                     
  DTL   FLOAT,                                      
  QCA   FLOAT,                                     
  QCG   FLOAT,                                      
  FTQ   FLOAT,                                
  QCD   FLOAT,                   
COQ   FLOAT,                                      
  OT   FLOAT,                                      
  [NOT]  FLOAT,                                      
  FPT   FLOAT,                                      
  O1   Float,                                      
  TPM   Float,                                      
  JB   VARCHAR(20),                                       
  EID   VARCHAR(20),                                    
  Comp nvarchar(50),                                    
  PPS int                                      
 )                                      
        
 INSERT INTO #tmpOEE21_1_Detail(OEEID, OEEDATE,IID,MODEID,  MID, CID,                                       
       O, A, P, Q, TSH, ASH, PPT, DTL, QCA, QCG, FTQ, QCD, COQ,                                       
       OT, [NOT], FPT, O1, TPM,                                      
       JB, EID,Comp,PPS )                                      
 SELECT OEEID, OEEDATE, IID, MODEID, MID,CID,                                      
   O, A, P, Q, TSH, ASH, PPT, DTL, QCA, QCG, FTQ, QCD, COQ,                                       
   OT, [NOT], FPT, O1, TPM, JB, EID,Comp,PPS                             
 FROM ( SELECT OEEID, OEEDATE,IID, MODEID,MID,CID,                                      
      O, A, P, Q, TSH, ASH, PPT, DTL, QCA, QCG, FTQ, QCD, COQ,                                      
      OT, [NOT], FPT, O1,TPM,                                      
      Opr1, Opr2,Opr3, Trim,Trim2, QC,QC2, ProdTL,ProdTL2, TrimTL,TrimTL2, QCTL ,Comp,PPS                                      
    FROM #tmpOEE21_Detail) p                                      
 UNPIVOT                                      
 (EID FOR JB IN (Opr1, Opr2,Opr3, Trim,Trim2, QC,QC2, ProdTL,ProdTL2, TrimTL,TrimTL2, QCTL))AS unpvt                                      
 WHERE EID <> '0'                                   
                                                                 
                                             
SELECT  opr,OEEDate,PPS,CID, COUNT(IID) as TOpr                                      
into #TOpr_Detail                                      
FROM  (select distinct IID,Opr,ShiftID,OEEDate,PPS,CID from @Emp                         
where Opr <> '0' and Opr is not null                                       
--group by  IID,ShiftID,Opr,OEEDate                                      
) A--@Emp                                      
--where  Opr <> '0' and Opr is not null                                       
GROUP BY opr,OEEDate,PPS,CID                   
                  --select distinct IID,Opr,ShiftID,OEEDate,PPS,CID from @Emp where Opr <> '0' and Opr is not null                        
-- Get Total Input of Trim                                      
SELECT  Trim,OEEDate,PPS,CID, COUNT(IID) as TTrim--COUNT(OEEID) as TTrim                                      
into #TTrim_Detail                                       
FROM (select distinct IID,Trim,ShiftID,OEEDate,CID,PPS from @Emp                         
where Trim <> '0' and Trim is not null                                       
group by  IID,ShiftID,Trim,OEEDate,PPS,CID) A --@Emp                          
where Trim <> '0' and Trim is not null                                       
GROUP BY Trim,OEEDate,PPS,CID                                  
                                  
-- Get Total Input of QC                     
SELECT  QC,OEEDate,PPS,CID, COUNT(IID) as TQC                                      
into #TQC_Detail                                      
FROM (select distinct IID,QC,ShiftID,OEEDate,PPS,CID from @Emp                        
where QC <> '0' and QC is not null                                       
group by  IID,ShiftID,QC,OEEDate,PPS,CID) A-- @Emp                                       
where QC <> '0' and QC is not null                                       
GROUP BY QC,OEEDate,PPS ,CID                                     
                                
--Select * from #tmpOEE21_1_Detail where LEFT(JB,3) = 'Opr'                                      
                          
-- get opr                                      
insert  @Emp1(IID,Opr,OEEDate,PPS,CID)                                      
SELECT     IID, opr,OEEDate,PPS,CID                                       
FROM   @Emp                                        
where      Opr <> '0' and Opr is not null                                       
GROUP BY IID , opr,OEEDate,PPS,CID                                      
                                    
insert  @Emp2(IID,Trim,OEEDate,PPS,CID)                                      
SELECT     IID, Trim,OEEDate,PPS,CID                                        
FROM    @Emp              
where      Trim <> '0'  and Trim is not null                                      
GROUP BY IID ,Trim,OEEDate,PPS ,CID                                      
                                
insert  @Emp3(IID,QC,OEEDate,PPS,CID )                                      
SELECT     IID,QC ,OEEDate,PPS,CID                                       
FROM         @Emp                           
where      QC <> '0'  and QC is not null                                      
GROUP BY IID ,QC,OEEDate,PPS ,CID                          
                          
                                                      
insert @Emp5(IID,OEEDate ,TOpr,TTrim,TQC,PPS,CID)                                      
select IID,OEEDate ,sum(OTOpr)as OTOpr, sum(OTTrim) as OTTrim, sum(OTQC) as OTQC,PPS as PPS,CID  as CID                          
                                  
from                                     
(                                      
select   A.IID as IID,A.OEEDate , case when E1.TOpr = 0 then 0 else sum(OT) end  As OTOpr, 0 as OTTrim, 0 as OTQC,E1.PPS,E1.CID    --/E1.TOpr                                 
FROM                                       
(Select * from #tmpOEE21_1_Detail where LEFT(jb,3) = 'Opr')A             
inner join (                                      
select A.IID,A.OEEDate ,O.TOpr,O.Opr,A.PPS,A.CID from @Emp1 A inner join #TOpr_Detail O on A.Opr = O.Opr and A.OEEDate = O.OEEDate  and A.PPS = O.PPS and A.CID = O.CID                                      
) E1 ON E1.Opr = A.EID and E1.IID = A.IID and E1.OEEDate = a.OEEDATE and E1.PPS = a.PPS and E1.CID = a.CID                                      
WHERE A.OT> 0 and A.OT is not null                                      
group by a.IID,e1.TOpr,A.OEEDate,E1.PPS,E1.CID --,a.EID,A.EID                                      
union                                   
select   A.IID as IID,A.OEEDate,0 AS OTOpr, case when E1.TTrim = 0 then 0 else sum(OT) end  As OTTrim, 0 as OTQ,E1.PPS,E1.CID                                        
FROM                                       
(Select * from #tmpOEE21_1_Detail  where LEFT(jb,4) = 'Trim')A                                      
inner join (                                      
select A.IID,A.OEEDate,T.TTrim,T.Trim,A.PPS,A.CID from @Emp2 A inner join #TTrim_Detail T on A.Trim = T.Trim and A.OEEDate = T.OEEDate   and A.PPS = T.PPS and A.CID = T.CID                                     
) E1 ON E1.Trim = A.EID and E1.IID = A.IID and E1.OEEDate = a.OEEDATE  and E1.PPS = a.PPS    and E1.CID = a.CID                                
WHERE A.OT> 0 and A.OT is not null                                      
group by a.IID,e1.TTrim,A.OEEDate,E1.PPS,E1.CID--,a.EID                                      
union                                     
select   A.IID as IID,A.OEEDate, 0 as OTOpr, 0 as OTQC, case when E1.TQC = 0 then 0 else sum(OT) end  As OTQC,E1.PPS,E1.CID                          
FROM                                       
(Select * from #tmpOEE21_1_Detail  where LEFT(jb,2) = 'QC') A                                      
inner join (                                      
select A.IID,A.OEEDate,Q.TQC,Q.QC,A.PPS,A.CID from @Emp3 A inner join #TQC_Detail Q on A.QC = Q.QC and A.OEEDate = Q.OEEDate and A.PPS = Q.PPS and A.CID = Q.CID                                        
) E1 ON E1.QC = A.EID and E1.IID = A.IID and E1.OEEDate = a.OEEDATE and E1.PPS = a.PPS   and E1.CID = a.CID                                       
WHERE A.OT> 0 and A.OT is not null                                      
group by a.IID,e1.TQC,A.OEEDate,E1.PPS,E1.CID--,a.EID                                      
)O group by IID,OEEDate,PPS,CID                                      
                                                     
  SELECT B.IID,                                      
                                                          
                                      
  MT,                                       
(isnull(SUM(CAST(A.A401 AS FLOAT)),0)+ isnull(sum(CAST(OT.A401 as Float)),0))/dbo.ISZERO(SUM(CAST(L301 AS FLOAT)),1) AS A,                                       
SUM(CAST(P401 AS FLOAT))/dbo.ISZERO(isnull(SUM(CAST(A.A401 AS FLOAT)),0)+ isnull(sum(CAST(OT.A401 as Float)),0),1) AS P,                                      
SUM(CAST(Q401 AS FLOAT))/dbo.ISZERO(SUM(CAST(P401 AS FLOAT)),1) AS Q,                                       
SUM(CAST(L301 AS FLOAT)) AS PPT,                                       
isnull(SUM(CAST(A.A301 AS FLOAT)),0)+isnull(sum(CAST(DT.A301 as Float)),0) AS DTL,            
SUM(CAST(COQ AS FLOAT)) AS COQ,                                       
((SUM(CAST(Q201 AS FLOAT)))/dbo.ISZERO((SUM(CAST(P401 AS FLOAT))),1)) AS RTO ,                                      
SUM(cast(Q401 as float)) as FPT ,                                      
sum(cast(L105 as float)) as O1,                                      
sum(cast(L103 as float)) as TPM,                                      
sum(cast(P401 as float)) as [NOT],                                      
sum(isnull(CAST(A.A401 AS FLOAT),0)) + sum(isnull(CAST(OT.A401 as Float),0)) AS OT,                                    
cmp.F000 as Comp,B.PPS,B.CTS--                                      
 into #tmpOEE16_12_Detail                                       
 FROM (select * from TBLOEEDETAIL WITH (NOLOCK))  A                                       
INNER JOIN (select * from tblOEEHeader WITH (NOLOCK) WHERE  DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME,  @v_FROMDATE))<= 0 AND DATEDIFF(MINUTE,OEEDATE,CONVERT(DATETIME,  @v_TODATE))>=0 ) B ON B.OEEID = A.F000                                       
right join #tmpOEE16_1_Detail T on T.IID = B.IID and t.CID = b.CID      and t.PPS = b.PPS                                 
left JOIN (SELECT DISTINCT F000 FROM TBLITEM WITH (NOLOCK) WHERE (F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID)))) I ON I.F000 = B.IID                                       
left JOIN (select TI as A401,OEEID from tblOEEDTProcess WITH (NOLOCK) where ProcessID = 'z4')OT on OT.OEEID = B.OEEID                                      
left JOIN (select TI as A301,OEEID from tblOEEDTProcess  WITH (NOLOCK) where ProcessID = 'z3')DT on DT.OEEID = B.OEEID                                      
                 
left join tblComp cmp on B.CID = cmp.F000                                       
WHERE OEETYPE = 'TI' AND DATEDIFF(MINUTE, B.OEEDATE, CONVERT(DATETIME,  @V_FROMDATE))<= 0 AND DATEDIFF(MINUTE, B.OEEDATE,CONVERT(DATETIME,  @V_TODATE))>=0                                       
group by B.iid,MT,B.PPS,B.CTS,cmp.F000                                  
                            
select B.IID,sum(E.TOpr) as Oprs,sum(E.TTrim) as Trims,sum(E.TQC) as QCs,MT,A,P,Q,PPT,DTL,COQ,RTO,FPT,O1,TPM,[NOT],OT,Comp,B.PPS,B.CTS                                           
into #tmpOEE16_2_Detail      
from (select * from #tmpOEE16_12_Detail) B                                      
left join (select IID, sum(TOpr) as TOpr,sum(TTrim) as TTrim ,sum (TQC) as TQC,PPS,CID from @Emp5 group by IID,PPS,CID ) E on B.IID = E.IID and B.PPS = E.PPS and B.Comp = E.CID -- and B.OEEDate = E.OEEDate                                      
group by B.IID,MT,A,P,Q,PPT,DTL,COQ,RTO,FPT,O1,TPM,[NOT],OT,Comp,B.PPS,B.CTS                                       
                           
  select * into #rpt16_Detail from (                                      
select distinct --a.CID,                                       
a.iid, c.F001,--a.TOpr,a.TTrim,a.TQC,                                      
 MT, (A * P * Q) AS O, A, P, Q,  PPT, DTL,  RTO,sum(QCG) as QCG, sum(QCD) as QCD, sum(SRE)as SRE,FPT, sum(O1) as O1, sum(TPM) as TPM,((PPT-isnull(MT,0)) )as runningtime  ,--good final step                                      
--a.cid                                      
sum(Oprs) as Oprs,sum(Trims) as Trims,sum(QCs) as QCs, [NOT],[OT],a.CTS,a.Comp as CID,cmp.F001 as Comp,a.PPS                    
From (select  distinct * from #tmpOEE16_2_Detail  ) a                                        
 inner  join (                                       
   select distinct SUM(CAST(a.Q202 AS FLOAT)) AS QCG, SUM(CAST(A.Q201 AS FLOAT)) AS QCD,(isnull(SUM(CAST(A.A124 AS FLOAT)),0) + ISNULL(SUM(CAST(SR.A124 AS FLOAT)),0)) AS SRE,                   
   b.IID,ic.F001,b.CID as CID,b.PPS,b.CTS                                       
   from (select * from tbloeedetail WITH (NOLOCK)  WHERE OEETYPE = @cateR  ) a                                       
   left join (select * from tbloeeheader WITH (NOLOCK) WHERE DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME,  @V_FROMDATE)) <= 0 AND DATEDIFF(MINUTE, OEEDATE,CONVERT(DATETIME,  @V_TODATE)) >=0) b on a.f000 = b.oeeid                                       
     left join  (select I.f000,I.f001, I.f004, c.f001 as CID,C.f005 from tblitem I inner join tblitemcomp c on i.f000 = c.f000                                       
     where (I.F004 in (select Id from dbo.Get_List_Category('',@V_CATEGORYID))  ) )ic on b.iid = Ic.f000 and b.cid = ic.cid                                       
  --and c.f005 = 1                                    
     left JOIN (select PA as A124,OEEID from tblOEEDTProcess WITH (NOLOCK) where DTID = 'A124')SR on SR.OEEID = b.OEEID                                      
     group by b.iid,ic.F001,b.CTS,b.CID,b.PPS                                      
   )c on a.iid = c.iid and a.Comp = c.cid   and a.PPS = c.PPS and a.CTS = c.CTS --                                    
   left join tblComp cmp on a.Comp = cmp.F000                                      
where c.F001  is not null                                      
group by a.iid, A, P, Q, PPT, DTL, RTO,FPT,O1,TPM,MT,coq,c.F001,[NOT],[OT],a.CTS,a.Comp,cmp.F001,a.PPS                                                          
 ) HT                                       
 where HT.OT >= 0                                      
                         
                                    print 'here'           
                                            
 select --*                      
 --r.iid, r.F001,--a.TOpr,a.TTrim,a.TQC,                                      
 --r.MT,r.O, r.A, r.P, r.Q,  r.PPT, r.DTL,  r.RTO,r.QCG,r.QCD,r.SRE,FPT,r.O1,r.TPM,r.runningtime              
 --,r.Oprs*(case when i.F011=0 then 1 else i.F011 end ) as Oprs,r.Trims*(case when i.F012=0 then 1 else i.F012 end ) as Trims,                        
 --r.QCs*(case when i.F013=0 then 1 else i.F013 end ) as QCs, r.[NOT],r.[OT],r.CTS,r.CID,r.PPS
 r.Comp                        
 ,case when PPS=0 and (QCG+QCD) > 0 then OT*60/(QCG+QCD)                                     
     when (QCG+QCD) = 0 and PPS>0 then OT*60/(1/PPS)                                      
   when (QCG+QCD) > 0 and PPS>0 then  OT*60 /((QCG+QCD)/PPS)                                       
    when (QCG+QCD) = 0 and PPS=0 then  OT                                      
 end  as ActCT_PS,                  
 case when (QCG+QCD) > 0 then OT*60/(QCG+QCD)                                    
     when (QCG+QCD)>0 then OT*60/1  end  as ActC,
	 r.CTS                                    
                                
 from #rpt16_Detail r  --where iid='6112041301'                                    
       inner join tblItemComp i on IID= i.F000 and CID = i.F001            
       where O >0                          
 order by iid,CId                                     
                                 
END;                                      
                                
                                    
--select * from tblItem where F000='6114061212' 