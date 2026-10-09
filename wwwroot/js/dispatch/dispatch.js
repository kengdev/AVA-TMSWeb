'use strict';
const $=s=>document.querySelector(s),S=FleetSchedule;
const tripLabels={PLANNED:'จัดแล้ว • ยังไม่เริ่ม',RUNNING:'กำลังวิ่ง',COMPLETED:'เสร็จแล้ว'};
let data={vehicles:[],jobs:[],assignments:[],preparationMinutes:30,turnaroundMinutes:45},selectedJob=null,selectedAssignment=null,fleet='ALL',loading=false,saving=false,toastTimer,dialogContext=null;
const esc=x=>String(x??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const today=()=>S.dateKey(new Date());
const time=x=>new Date(x).toLocaleTimeString('th-TH',{timeZone:'Asia/Bangkok',hour:'2-digit',minute:'2-digit',hourCycle:'h23'});
const dateTime=x=>new Date(x).toLocaleString('th-TH',{timeZone:'Asia/Bangkok',day:'numeric',month:'short',hour:'2-digit',minute:'2-digit',hourCycle:'h23'});
const jobById=id=>data.jobs.find(x=>x.id===id),vehicleById=id=>data.vehicles.find(x=>x.id===id);
const selected=()=>jobById(selectedJob);
const activeDay=()=>$('#workDate').value||today();
function notify(message,error=false){$('#toast').textContent=message;$('#toast').className=error?'error':'';$('#toast').style.display='block';clearTimeout(toastTimer);toastTimer=setTimeout(()=>$('#toast').style.display='none',5500);}
function check(v,j,p=j?.pickup,d=j?.delivery,ignore=null){return S.check(v,j,p,d,data.assignments,data.preparationMinutes,data.turnaroundMinutes,ignore);}
function assignmentsForDay(day){const b=S.dayBounds(day);return data.assignments.filter(a=>S.overlaps(S.millis(a.occupiedFrom),S.millis(a.availableAt),b.start,b.end));}
async function reload(){
 if(loading||saving)return;loading=true;$('#refresh').disabled=true;
 try{const r=await fetch(window.dispatchConfig.data,{cache:'no-store'});if(!r.ok)throw new Error('โหลดข้อมูลไม่สำเร็จ');data=await r.json();
  if(data.assignments.some(x=>x.jobId===selectedJob))selectedJob=null;
  if(!data.assignments.some(x=>x.id===selectedAssignment))selectedAssignment=null;
  $('#prepMinutes').textContent=data.preparationMinutes;$('#turnMinutes').textContent=data.turnaroundMinutes;render();$('#updated').textContent='อัปเดต '+time(new Date())+' (เวลาไทย)';
 }catch(e){notify(e.message,true);}finally{loading=false;$('#refresh').disabled=false;}
}
async function post(action,payload){
 if(saving)return false;if(loading)throw new Error('กำลังรีเฟรชข้อมูล กรุณาลองใหม่เมื่อโหลดเสร็จ');saving=true;$('#refresh').disabled=true;
 try{const r=await fetch(window.dispatchConfig[action],{method:'POST',headers:{'Content-Type':'application/json','RequestVerificationToken':$('input[name="__RequestVerificationToken"]').value},body:JSON.stringify(payload)});
  let result;try{result=await r.json();}catch{throw new Error('เซิร์ฟเวอร์ไม่ตอบกลับ กรุณารีเฟรชก่อนลองอีกครั้ง');}
  if(!r.ok)throw new Error(result.message||'บันทึกไม่สำเร็จ');notify(result.message);return true;
 }finally{saving=false;$('#refresh').disabled=false;}
}
function render(){
 const day=activeDay(),dayTrips=assignmentsForDay(day),pending=data.jobs.filter(j=>S.dateKey(j.pickup)===day&&!data.assignments.some(a=>a.jobId===j.id)).sort((a,b)=>Number(b.urgent)-Number(a.urgent)||S.millis(a.pickup)-S.millis(b.pickup));
 const counts=[['รถทั้งหมด',data.vehicles.length,'#193c5e','บริษัท + รถร่วม'],['รถมีเที่ยวในวันนี้',new Set(dayTrips.map(a=>a.vehicleId)).size,'#3973e8','รวมเที่ยวข้ามวัน'],['เที่ยวที่จัดแล้ว',dayTrips.length,'#18a58c','ทุกสถานะของวันที่เลือก'],['งานรอจัดรถ',pending.length,'#db9e33','ตามวันรับสินค้า'],['รถร่วมรอยืนยัน',data.vehicles.filter(v=>v.status==='UNCONFIRMED').length,'#bd8e38','ต้องยืนยันก่อนจัด'],['รถซ่อมบำรุง',data.vehicles.filter(v=>v.status==='MAINTENANCE').length,'#8997a6','ยังจัดงานไม่ได้']];
 $('#metrics').innerHTML=counts.map(([label,n,c,note])=>`<div class="metric" style="--color:${c}"><small>${label}</small><strong>${n}</strong><em>${note}</em></div>`).join('');
 $('#dateLabel').textContent=new Date(day+'T12:00:00+07:00').toLocaleDateString('th-TH',{timeZone:'Asia/Bangkok',weekday:'short',day:'numeric',month:'short',year:'numeric'});
 $('#jobCount').textContent=pending.length;
 $('#jobs').innerHTML=pending.map(j=>`<button class="job ${selectedJob===j.id?'selected':''}" data-job="${esc(j.id)}" aria-pressed="${selectedJob===j.id}"><div class="job-top"><span class="job-id">${esc(j.id)}</span>${j.urgent?'<span class="urgent">งานด่วน</span>':''}</div><div class="customer">${esc(j.customer)}</div><div class="route">จาก ${esc(j.origin)}<br>ไป ${esc(j.destination)}</div><div class="job-bottom"><span>${esc(j.kind)}</span><span>${time(j.pickup)} → ${time(j.delivery)}${S.dateKey(j.delivery)!==S.dateKey(j.pickup)?' (+1วัน)':''}</span></div><div class="job-ready">พร้อมเที่ยวถัดไป ${dateTime(S.millis(j.delivery)+data.turnaroundMinutes*S.minute)}</div></button>`).join('')||'<div class="empty">ไม่มีงานรอจัดรถในวันที่เลือก<br>Demo มีงานวันนี้และอีก 2 วัน</div>';
 $('#clearJob').hidden=!selectedJob;renderTimeline();renderDetails();
}
function renderTimeline(){
 const day=activeDay(),j=selected(),q=$('#search').value.trim().toLowerCase(),status=$('#status').value;
 const vs=data.vehicles.filter(v=>(fleet==='ALL'||v.fleetType===fleet)&&(status==='ALL'||status===v.status||status==='AVAILABLE'&&j&&check(v,j).ok)&&[v.plate,v.driver,v.partner,v.location].some(t=>t.toLowerCase().includes(q)));
 $('#vehicleCount').textContent=vs.length;
 $('#fleetHint').textContent=j?`กำลังจัด ${j.id} • รับ ${time(j.pickup)} ถึงพร้อม ${dateTime(S.millis(j.delivery)+data.turnaroundMinutes*S.minute)}`:'เวลาไทย (UTC+7) • เลื่อนแนวนอนเพื่อดูทั้งวัน';
 const startHour=Number($('#hourRange').value),hourWidth=Number($('#zoom').value),bounds=S.dayBounds(day,startHour),width=(24-startHour)*hourWidth;
 const timeline=$('#timeline');timeline.style.setProperty('--hour-width',hourWidth+'px');timeline.style.width=(width+192)+'px';
 let html=`<div class="timeline-header"><div class="label-header">รถ / คนขับ</div><div class="hours" style="width:${width}px">${Array.from({length:24-startHour},(_,i)=>`<div class="hour" style="width:${hourWidth}px">${String(i+startHour).padStart(2,'0')}:00</div>`).join('')}${nowMarkup(bounds,width,true)}</div></div>`;
 let visibleTrips=0;
 for(const group of ['COMPANY','PARTNER']){
  const rows=vs.filter(v=>v.fleetType===group);if(!rows.length)continue;
  html+=`<div class="group-row" style="width:${width+192}px"><div class="group-label">${group==='COMPANY'?'รถบริษัท':'รถร่วมขนส่ง'} • ${rows.length} คัน</div></div>`;
  for(const v of rows){
   const availability=j?check(v,j):null;
   const statusText=v.status==='MAINTENANCE'?'ซ่อมบำรุง':v.status==='UNCONFIRMED'?'รถร่วมรอยืนยัน':j?(availability.ok?'จัดงานนี้':availability.message):'เลือกงานเพื่อเช็คเวลาว่าง';
   const action=v.status==='UNCONFIRMED'?`<button class="row-action" data-confirm="${esc(v.id)}">ยืนยันรถร่วม</button>`:`<button class="row-action ${availability&&!availability.ok?'blocked':''}" data-vehicle="${esc(v.id)}" ${!availability?.ok?'disabled':''} title="${esc(statusText)}">${esc(statusText)}</button>`;
   html+=`<div class="vehicle-row"><div class="vehicle-label"><strong>${esc(v.plate)}</strong><small>${esc(v.driver)} • ${esc(v.kind)}</small><small>${group==='PARTNER'?esc(v.partner):esc(v.location)}</small>${action}</div><div class="lane ${v.status!=='READY'?'closed':''} ${availability?.ok?'selectable':''}" data-lane="${esc(v.id)}" style="width:${width}px">`;
   if(v.status==='MAINTENANCE')html+='<span class="lane-message">ซ่อมบำรุง • ยังไม่พร้อมจัดงาน</span>';
   if(v.status==='UNCONFIRMED')html+='<span class="lane-message">รอการตอบรับจากรถร่วม</span>';
   for(const a of data.assignments.filter(a=>a.vehicleId===v.id)){
    const buffer=S.projection(S.millis(a.occupiedFrom),S.millis(a.availableAt),bounds.start,bounds.end,width);if(!buffer)continue;
    const trip=S.projection(S.millis(a.pickupAt),S.millis(a.deliveryAt),bounds.start,bounds.end,width),job=jobById(a.jobId);visibleTrips++;
    html+=`<div class="occupied-bar" style="left:${buffer.left}px;width:${buffer.width}px" title="จอง ${esc(dateTime(a.occupiedFrom))}–${esc(dateTime(a.availableAt))}"></div>`;
    const box=trip||buffer;
    html+=`<button class="trip-bar ${esc(a.status)} ${selectedAssignment===a.id?'selected':''}" data-assignment="${esc(a.id)}" style="left:${box.left}px;width:${box.width}px" title="${esc(a.jobId)} • ${esc(job?.customer)}\n${esc(dateTime(a.pickupAt))} → ${esc(dateTime(a.deliveryAt))}\nพร้อม ${esc(dateTime(a.availableAt))}" aria-label="${esc(a.jobId)} ${esc(job?.customer)} ${esc(dateTime(a.pickupAt))} ถึง ${esc(dateTime(a.deliveryAt))}"><b>${box.clippedLeft?'‹ ':''}${esc(a.jobId)} • ${esc(job?.customer)}${box.clippedRight?' ›':''}</b><small>${esc(job?.origin)} → ${esc(job?.destination)} • ${time(a.pickupAt)}–${time(a.deliveryAt)}</small></button>`;
   }
   if(j){const range=S.interval(j.pickup,j.delivery,data.preparationMinutes,data.turnaroundMinutes),ghost=S.projection(range.from,range.until,bounds.start,bounds.end,width);if(ghost)html+=`<div class="ghost ${availability.ok?'':'conflict'}" style="left:${ghost.left}px;width:${ghost.width}px"><span>${esc(j.id)} ${availability.ok?'• ว่าง':'• จัดไม่ได้'}</span></div>`;}
   html+=nowMarkup(bounds,width,false)+'</div></div>';
  }
 }
 if(!vs.length)html+='<div class="empty">ไม่พบรถตามตัวกรอง'+(status==='AVAILABLE'&&!j?' • กรุณาเลือกงานก่อน':'')+'</div>';
 timeline.innerHTML=html;$('#timelineSummary').textContent=`แสดง ${vs.length} คัน • ${visibleTrips} เที่ยวในช่วงเวลานี้`;
}
function nowMarkup(bounds,width,header){const now=Date.now();if(now<bounds.start||now>=bounds.end)return '';const left=(now-bounds.start)/(bounds.end-bounds.start)*width;return header?`<span class="now-label" style="left:${left}px">${time(now)}</span>`:`<div class="now-line" style="left:${left}px"></div>`;}
function detailRow(label,value){return `<div class="detail-item"><span>${esc(label)}</span><strong>${esc(value)}</strong></div>`;}
function renderDetails(){
 const a=data.assignments.find(x=>x.id===selectedAssignment),j=a?jobById(a.jobId):selected(),v=a?vehicleById(a.vehicleId):null;
 if(!j){$('#details').innerHTML='<div class="empty">เลือกงานฝั่งซ้าย<br>หรือคลิกเที่ยวงานบน Timeline</div>';return;}
 let html=`<span class="detail-status">${a?tripLabels[a.status]:'งานรอจัดรถ'}</span><h3>${esc(j.id)}</h3><div class="customer">${esc(j.customer)}</div><div class="detail-route">จาก ${esc(j.origin)}<br>ไป ${esc(j.destination)}</div>`;
 if(a){html+=detailRow('ทะเบียนรถ',v?.plate)+detailRow('คนขับ',a.driver)+detailRow('ประเภทรถ',j.kind)+detailRow('เตรียมรถ',dateTime(a.occupiedFrom))+detailRow('รับสินค้า',dateTime(a.pickupAt))+detailRow('คาดว่าส่งเสร็จ',dateTime(a.deliveryAt));
  html+=`<div class="detail-ready">พร้อมเที่ยวถัดไป<br><strong>${esc(dateTime(a.availableAt))}</strong></div>`;
  if(a.status==='PLANNED')html+=`<button class="full primary" data-edit="${esc(a.id)}">เปลี่ยนรถ / เวลา</button><button class="full danger" data-cancel="${esc(a.id)}">ยกเลิกการจัดรถ</button>`;
  else html+='<div class="detail-note">เที่ยวที่เริ่มแล้วหรือเสร็จแล้วจะแก้ไขและยกเลิกผ่านหน้าจอนี้ไม่ได้</div>';
 }else{html+=detailRow('ต้องการรถ',j.kind)+detailRow('รับสินค้า',dateTime(j.pickup))+detailRow('คาดว่าส่งเสร็จ',dateTime(j.delivery));
  const available=data.vehicles.filter(v=>check(v,j).ok).length;html+=`<div class="detail-ready">ว่างตามช่วงเวลางานนี้ <strong>${available} คัน</strong><br>รวมทั้งรถและคนขับ พร้อมเวลาเตรียมรถ/พัก</div><button class="full primary" id="chooseVehicle">เลือกรถ / ปรับเวลา</button>`;
 }
 html+='<div class="detail-note">เวลาส่งเสร็จและพร้อมเที่ยวถัดไปเป็นประมาณการ ไม่มีการคำนวณจาก GPS หรือเส้นทางจริง</div>';$('#details').innerHTML=html;
}
function openDialog(vehicleId=null,assignmentId=null){
 const a=data.assignments.find(x=>x.id===assignmentId),j=a?jobById(a.jobId):selected();if(!j)return;
 dialogContext={jobId:j.id,assignmentId:a?.id||null};
 $('#dialogTitle').textContent=a?'เปลี่ยนรถ / เวลาเที่ยว':'ยืนยันจัดรถ';$('#confirmAssign').textContent=a?'บันทึกการเปลี่ยนแปลง':'ยืนยันจัดรถ';
 $('#confirmation').innerHTML=`<div class="confirm-card"><b>${esc(j.id)} • ${esc(j.customer)}</b>${esc(j.origin)} → ${esc(j.destination)}<br>ต้องการ ${esc(j.kind)}</div>`;
 const candidates=data.vehicles.filter(v=>v.kind===j.kind),chosen=vehicleId||a?.vehicleId||candidates.find(v=>check(v,j).ok)?.id||candidates[0]?.id;
 $('#vehicleChoice').innerHTML=candidates.map(v=>`<option value="${esc(v.id)}" ${v.id===chosen?'selected':''}>${esc(v.plate)} • ${esc(v.driver)} • ${v.fleetType==='COMPANY'?'รถบริษัท':esc(v.partner)}${v.status!=='READY'?' • '+(v.status==='UNCONFIRMED'?'รอยืนยัน':'ซ่อม'):''}</option>`).join('');
 $('#pickupInput').value=S.localInput(a?.pickupAt||j.pickup);$('#deliveryInput').value=S.localInput(a?.deliveryAt||j.delivery);updateDialogCheck();$('#assignDialog').showModal();
}
function updateDialogCheck(){
 if(!dialogContext)return;
 const result=check(vehicleById($('#vehicleChoice').value),jobById(dialogContext.jobId),S.fromInput($('#pickupInput').value),S.fromInput($('#deliveryInput').value),dialogContext.assignmentId);
 $('#scheduleCheck').className='schedule-check'+(result.ok?'':' error');$('#scheduleCheck').textContent=result.message+(result.ok?` • จอง ${dateTime(result.from)} ถึง ${dateTime(result.until)}`:'');$('#confirmAssign').disabled=saving||!result.ok;
}
function changeDay(day){$('#workDate').value=day;selectedJob=null;selectedAssignment=null;render();}
$('#workDate').value=today();
$('#jobs').addEventListener('click',e=>{const b=e.target.closest('[data-job]');if(b){selectedJob=b.dataset.job;selectedAssignment=null;render();}});
$('#timeline').addEventListener('click',async e=>{
 const trip=e.target.closest('[data-assignment]');if(trip){selectedAssignment=trip.dataset.assignment;selectedJob=null;render();return;}
 const confirm=e.target.closest('[data-confirm]');if(confirm){if(saving||!window.confirm('ได้รับการตอบรับจากรถร่วมและพร้อมใช้งานแล้วใช่หรือไม่?'))return;confirm.disabled=true;try{await post('confirmPartner',confirm.dataset.confirm);await reload();}catch(err){notify(err.message,true);confirm.disabled=false;}return;}
 const button=e.target.closest('[data-vehicle]'),lane=e.target.closest('[data-lane]'),id=button?.dataset.vehicle||lane?.dataset.lane;
 if(id&&selected()){const result=check(vehicleById(id),selected());if(result.ok)openDialog(id);else notify(result.message,true);}
});
$('#details').addEventListener('click',async e=>{
 if(e.target.closest('#chooseVehicle')){openDialog();return;}
 const edit=e.target.closest('[data-edit]');if(edit){openDialog(null,edit.dataset.edit);return;}
 const cancel=e.target.closest('[data-cancel]');if(!cancel||saving||!window.confirm('ยกเลิกเที่ยวนี้ และคืนช่วงเวลาจองให้รถและคนขับ?'))return;cancel.disabled=true;
 try{await post('cancel',cancel.dataset.cancel);selectedAssignment=null;await reload();}catch(err){notify(err.message,true);cancel.disabled=false;}
});
$('#assignForm').addEventListener('submit',async e=>{
 e.preventDefault();if(saving||!dialogContext)return;const button=$('#confirmAssign');button.disabled=true;
 try{const c=dialogContext,payload={vehicleId:$('#vehicleChoice').value,pickupAt:S.fromInput($('#pickupInput').value),deliveryAt:S.fromInput($('#deliveryInput').value)};
  const valid=check(vehicleById(payload.vehicleId),jobById(c.jobId),payload.pickupAt,payload.deliveryAt,c.assignmentId);if(!valid.ok)throw new Error(valid.message);
  if(c.assignmentId)payload.assignmentId=c.assignmentId;else payload.jobId=c.jobId;
  const ok=await post(c.assignmentId?'move':'assign',payload);if(ok){$('#assignDialog').close();selectedJob=null;await reload();}
 }catch(err){notify(err.message,true);}finally{updateDialogCheck();}
});
$('#closeDialog').onclick=()=>$('#assignDialog').close();
for(const s of ['#vehicleChoice','#pickupInput','#deliveryInput'])$(s).addEventListener('input',updateDialogCheck);
$('#fleetTabs').addEventListener('click',e=>{const b=e.target.closest('[data-fleet]');if(!b)return;fleet=b.dataset.fleet;document.querySelectorAll('[data-fleet]').forEach(x=>x.classList.toggle('active',x===b));renderTimeline();});
$('#clearJob').onclick=()=>{selectedJob=null;render();};$('#search').oninput=renderTimeline;$('#status').onchange=renderTimeline;$('#zoom').onchange=renderTimeline;$('#hourRange').onchange=renderTimeline;
$('#workDate').onchange=()=>changeDay(activeDay());$('#today').onclick=()=>changeDay(today());
$('#prevDay').onclick=()=>changeDay(S.dateKey(S.millis(activeDay()+'T12:00:00+07:00')-24*60*S.minute));
$('#nextDay').onclick=()=>changeDay(S.dateKey(S.millis(activeDay()+'T12:00:00+07:00')+24*60*S.minute));
$('#refresh').onclick=reload;
setInterval(()=>{if(!loading&&!saving)renderTimeline();},60000);
reload();
