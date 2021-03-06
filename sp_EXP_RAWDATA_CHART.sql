USE [OEE]
GO
/****** Object:  StoredProcedure [dbo].[sp_EXP_RAWDATA_CHART]    Script Date: 12/26/2019 2:27:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
      
-- [EXP_RAWDATA] '2019-01-15','2019-02-15','0','nqhuy'        
ALTER PROCEDURE [dbo].[sp_EXP_RAWDATA_CHART]        
(         
 @V_FROMDATE  DATETIME,        
        
 @V_TODATE  DATETIME,        
        
 @V_IID   VARCHAR(20),        
        
 @V_USERID  VARCHAR(20)        
        
)        
        
AS            
BEGIN

SET FMTONLY OFF;  
-- EXEC sp_dbcmptlevel 'OEE', 90        
        
 DECLARE @ParmDefinition nvarchar(500);        
        
 SET @ParmDefinition = N'@OEEType VARCHAR(2), @FROMDATE DATETIME, @TODATE DATETIME, @IID VARCHAR(20),@signal varchar(1),@Type varchar(2)';        
        
 DECLARE @CList NVARCHAR(MAX), @Clist1 nvarchar(4000),@CList2 nvarchar(4000)        
        
 DECLARE @SQLStringTI  NVARCHAR(MAX)  ,@SQLStringPA  NVARCHAR(MAX)      
        
 SELECT  @Clist  = STUFF(( SELECT distinct        
        
         '],[' + SName + '-'+ ProcessID + ''         
        
       FROM tblOEEDTProcess P WITH (NOLOCK) inner join tblOEEHeader H WITH (NOLOCK) on H.OEEID = P.OEEID        
        
       inner join tblOEETemplate T WITH (NOLOCK) on P.DTID = T.ID         
        
       WHERE DATEDIFF(MINUTE, STARTTIME,CONVERT(DATETIME, @V_FROMDATE)) <=0 AND DATEDIFF(MINUTE, ENDTIME,CONVERT(DATETIME, @V_TODATE)) >=0 and (H.IID = @V_IID or @V_IID = '0') and left(ProcessID,1) <>''        
        
       ORDER BY '],[' + SName +'-'+ ProcessID + ''           
        
       FOR XML PATH('')        
        
          ), 1, 2, '') + ']'        
        
 SELECT  @CList1 = STUFF(( SELECT DISTINCT        
        
         '],ISNULL(B.[' + ID + '],0) AS [' + SNAME + ''        
        
       FROM    TBLOEETEMPLATE WITH (NOLOCK)       
        
       WHERE [Level] <= 4 and Kind in (1,2)   and Active=1      
        
       ORDER BY '],ISNULL(B.[' + ID + '],0) AS [' + SNAME + ''        
        
       FOR XML PATH('')        
        
          ), 1, 2, '') + ']'        
        
 SELECT  @CList2 = STUFF(( SELECT DISTINCT        
        
         '],ISNULL(B.[' + ID + '],0) AS [' + SNAME + Cast([Level] as varchar(1))+''        
        
       FROM    TBLOEETEMPLATE   WITH (NOLOCK)     
        
       WHERE [Level] <= 4 and Kind in (4,5)  and Active=1      
        
       ORDER BY '],ISNULL(B.[' + ID + '],0) AS [' + SNAME + cast([Level] as varchar(1))+ ''        
        
       FOR XML PATH('')        
        
          ), 1, 2, '') + ']'        
  print @CList1  
 Set @SQLStringTI = 'select [OT-z4], [PPT3], [NOT4], [QCG], [B_Output], [Category] from (SELECT B.*, A.* from '+        
        
 '(SELECT OEEID,'+@Clist+' FROM '+        
        
 '(SELECT  case when @OEEType = @Type then P.TI else P.PA end  as TI,SName+@signal + processID as DTID, 
 H.oeeid as OEEID FROM tblOEEDTProcess P WITH (NOLOCK) inner join tblOEEHeader H WITH (NOLOCK) 
 on H.OEEID=P.OEEID inner join tblOEETemplate T WITH (NOLOCK) on P.DTID=T.ID         
 WHERE DATEDIFF(MINUTE, STARTTIME, CONVERT(DATETIME, @FROMDATE)) <=0 AND DATEDIFF(MINUTE, ENDTIME,CONVERT(DATETIME, @TODATE)) >=0 and (H.IID=@IID or @IID=convert(varchar,0))) AS UP '+        
        
' PIVOT '+        
        
'(SUM(TI) FOR DTID IN ('+@Clist+')) AS PivotTable)A, ' +           
        
 '(SELECT CONVERT(VARCHAR, A.OEEDate, 101) AS OEEDate,'+         
       'A.OEEID as OEEIDDeatail, S.F001 AS Shift,'+         
       'A.ShiftName,'+         
       'D.F001 AS Mode,'+         
       'C.F001 AS Machine,'+         
       'A.CELL AS Cell,'+         
       --'T.F001 AS [Time]    ,'+         
       ' CASE WHEN status is null THEN (T.F001) ELSE LEFT(CAST(CONVERT(DATETIME,StartTime) AS VARCHAR),5) + '' - '' + LEFT(CAST(CONVERT(DATETIME,ENDTIME) AS VARCHAR),5)  END AS [Time],'+
        
       'A.OEER AS [OEEReporter],'+         
        
       'A.IID,'+         
        
       'A.SQT,'+        
        
       'A.CTS AS [CycleTimeinsecond],'+         
        
       'A.CTM AS [CycleTimeinminutes],'+         
        
       'dbo.GET_ITEMNAME(A.IID) AS Item,'+         
          'dbo.GET_CATEGORYNAME(ite.F004) AS [Category],'+ 
       'dbo.GET_COMPNAME(A.CID) AS Comp,'+         
        
       'Case when ite.F004=''001'' then ''True'' else I.F005 end  AS FStep,'+    
             
       'I.F033 AS MoldType,'+    
        
       'dbo.GET_COLORNAME_LongText(A.IID+''-''+IC.f003) as Color,'+    
        
       'dbo.GET_SIZENAMELISTNEW(A.SiID,A.SiID2,A.SiID3,A.SiID4,A.SiID5,A.IID) AS [Size],'+         
        
       'dbo.GET_SIZE_QUANTITY(A.SiID,A.SiID2,A.SiID3,A.SiID4,A.SiID5) AS [SizeQuantity],'+         
          'convert(varchar(50),A.CurrentTime) as CurrentTime , '+
            
       'dbo.GET_COLOR_QUANTITY(A.CoID,A.CoID2,A.CoID3) AS ColorQuantity, A.MSN as MoldSetNo, A.PPS as PairsPerShot,(Cast(B.[Q201] as float)  + cast(B.[Q202] as float)) as B_Output,
       ISNULL(PK.quantity,0) quantity,
       ' + @CList1 + char(13) +','+ @CList2 +        
     
       ' FROM TBLOEEDETAIL B WITH (NOLOCK)'+         

       'INNER JOIN TBLOEEHEADER A WITH (NOLOCK) ON A.OEEID=B.F000 '+        
       'INNER JOIN tblItem ite WITH (NOLOCK) ON A.IID=ite.F000 '+  
       'INNER JOIN TBLItemComp I WITH (NOLOCK) ON A.IID=I.F000 and A.CID=I.F001 '+        
        'Left join tblItemColor IC WITH (NOLOCK) ON IC.F000=A.IID and IC.F001=A.CoID '+  
       'LEFT OUTER JOIN TBLSHIFT S WITH (NOLOCK) ON S.F000=A.ShiftID '+         
        
       'LEFT OUTER JOIN TBLMACHINE C WITH (NOLOCK) ON C.F000=A.MID '+         
        
       'LEFT OUTER JOIN TBLMODE D WITH (NOLOCK) ON D.F000=A.MODEID '+         
        
       'LEFT OUTER JOIN TBLTIME T WITH (NOLOCK) ON T.F000=A.TIMEID '+         
        
       'LEFT JOIN (SELECT SUM(quantity) quantity,OEEID FROM tblOEEPackgedQuality WITH (NOLOCK)  GROUP BY OEEID)PK  ON PK.OEEID =A.OEEID '+   
       'WHERE B.OEETYPE=@OEEType '+         
       'AND DATEDIFF(MINUTE,A.STARTTIME,@FROMDATE)<= 0 '+         
        
       'AND DATEDIFF(MINUTE,A.STARTTIME,@TODATE)>=0 '+         
        
       'AND (A.IID=@IID OR @IID=convert(varchar,0)) '+         
        
     ')B where A.OEEID=B.OEEIDDeatail) CC inner join ( SELECT B.OEEID,      
      (isnull(SUM(CAST(A.A401 AS FLOAT)),0)+ isnull(SUM(CAST(OT.A401 as Float)),0))/dbo.ISZERO(SUM(CAST(A.L301 AS FLOAT)),1) AS A,      
      SUM(CAST(A.P401 AS FLOAT))/dbo.ISZERO(isnull(SUM(CAST(A.A401 as Float)),0)+isnull(SUM(CAST(OT.A401 as Float)),0),1) AS P,      
      SUM(CAST(A.Q401 AS FLOAT))/dbo.ISZERO(SUM(CAST(A.P401 AS FLOAT)),1) AS Q      
 FROM (SELECT * FROM TBLOEEDETAIL WITH (NOLOCK)  WHERE OEETYPE =@OEEType ) A     
 INNER JOIN (SELECT * FROM TBLOEEHEADER WITH (NOLOCK)  WHERE DATEDIFF(MINUTE, STARTTIME,CONVERT(DATETIME, @FROMDATE)) <=0 AND DATEDIFF(MINUTE, ENDTIME,CONVERT(DATETIME, @TODATE)) >=0 ) B ON B.OEEID = A.F000      
                   left JOIN (SELECT TI as A401,OEEID from tblOEEDTProcess WITH (NOLOCK) where ProcessID = ''z4'')OT on OT.OEEID = B.OEEID      
                   left JOIN (SELECT TI as A301,OEEID from tblOEEDTProcess WITH (NOLOCK) where ProcessID = ''z3'')DT on DT.OEEID = B.OEEID      
                INNER JOIN (SELECT DISTINCT F000 FROM TBLITEM ) I ON I.F000 = B.IID      
 GROUP BY B.OEEID )OO on CC.OEEID = OO.OEEID ORDER BY CC.CurrentTime, CC.OEEDate,CC.MODE,CC.SHIFT,CC.[TIME],CC.Machine;'        
    
 EXEC sp_executesql @SQLStringTI, @ParmDefinition, @OEEType = 'TI', @FROMDATE = @V_FROMDATE, @TODATE = @V_TODATE, @IID = @V_IID,@signal = '-',@Type = 'TI'        
   print @SQLStringTI      
         
END; 
