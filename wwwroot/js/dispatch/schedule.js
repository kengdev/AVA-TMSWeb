/* Shared pure UI scheduling functions; server revalidates independently. */
(function(root,factory){const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.FleetSchedule=api;})(typeof globalThis!=='undefined'?globalThis:this,function(){
 'use strict';
 const minute=60000;
 function millis(value){return new Date(value).getTime();}
 function dateKey(value){const shifted=new Date(millis(value)+7*60*minute);return shifted.toISOString().slice(0,10);}
 function localInput(value){return new Date(millis(value)+7*60*minute).toISOString().slice(0,16);}
 function fromInput(value){return value?value+':00+07:00':null;}
 function dayBounds(day,startHour=0){return {start:millis(day+'T'+String(startHour).padStart(2,'0')+':00:00+07:00'),end:millis(day+'T00:00:00+07:00')+24*60*minute};}
 function overlaps(from,until,otherFrom,otherUntil){return from<otherUntil&&until>otherFrom;}
 function interval(pickup,delivery,prep,turn){return {from:millis(pickup)-prep*minute,until:millis(delivery)+turn*minute};}
 function check(vehicle,job,pickup,delivery,assignments,prep,turn,ignoreId=null){
  if(!vehicle||!job)return {ok:false,message:'เลือกงานและรถก่อน'};
  const p=millis(pickup),d=millis(delivery);
  if(!Number.isFinite(p)||!Number.isFinite(d)||d<=p)return {ok:false,message:'เวลาจบงานต้องอยู่หลังเวลารับสินค้า'};
  if(d-p>48*60*minute)return {ok:false,message:'ตัวอย่างนี้รองรับเที่ยวงานไม่เกิน 48 ชั่วโมง'};
  if(vehicle.status==='MAINTENANCE')return {ok:false,message:'รถอยู่ระหว่างซ่อมบำรุง'};
  if(vehicle.status==='UNCONFIRMED')return {ok:false,message:'ต้องยืนยันรถร่วมก่อน'};
  if(vehicle.kind!==job.kind)return {ok:false,message:'ประเภทรถไม่ตรงงาน'};
  if(!vehicle.driver?.trim())return {ok:false,message:'รถยังไม่มีคนขับ'};
  const range=interval(pickup,delivery,prep,turn);
  const conflict=assignments.find(a=>a.id!==ignoreId&&(a.vehicleId===vehicle.id||a.driver===vehicle.driver)&&overlaps(range.from,range.until,millis(a.occupiedFrom),millis(a.availableAt)));
  return conflict?{ok:false,message:'ซ้อนกับ '+conflict.jobId+' (รวมเตรียมรถ/พัก)',conflictId:conflict.id,...range}:{ok:true,message:'ช่วงเวลาว่าง จัดเที่ยวนี้ได้',...range};
 }
 function projection(from,until,start,end,width){if(!overlaps(from,until,start,end))return null;return {left:(Math.max(from,start)-start)/(end-start)*width,width:(Math.min(until,end)-Math.max(from,start))/(end-start)*width,clippedLeft:from<start,clippedRight:until>end};}
 return {minute,millis,dateKey,localInput,fromInput,dayBounds,overlaps,interval,check,projection};
});
