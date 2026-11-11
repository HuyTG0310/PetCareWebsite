from xml.sax.saxutils import escape
import xml.etree.ElementTree as ET

def q(s):
    return escape(s, {'"': '&quot;'}).replace('\n', '&#xa;')

BOX = "rounded=0;whiteSpace=wrap;html=1;strokeColor=#000000;strokeWidth=1.5;fillColor=#ffffff;fontFamily=Arial;fontSize=12;"
TXT = "text;html=1;align={a};verticalAlign=middle;fontSize=11;fontFamily=Arial;"
cells = []

def v(id, val, style, x, y, w, h):
    cells.append(f'<mxCell id="{id}" value="{q(val)}" style="{style}" vertex="1" parent="1"><mxGeometry x="{x}" y="{y}" width="{w}" height="{h}" as="geometry"/></mxCell>')

def e(id, x1, y1, x2, y2, arrow):
    st = "endArrow=classic;html=1;strokeColor=#000000;strokeWidth=1.2;" if arrow else "endArrow=none;html=1;strokeColor=#000000;strokeWidth=1.5;"
    cells.append(f'<mxCell id="{id}" style="{st}" edge="1" parent="1"><mxGeometry relative="1" as="geometry"><mxPoint x="{x1}" y="{y1}" as="sourcePoint"/><mxPoint x="{x2}" y="{y2}" as="targetPoint"/></mxGeometry></mxCell>')

def diagram(p, oy, title, actor, view, ctrl, hdl, repo, m):
    v(p+"t", title, "text;html=1;fontSize=16;fontStyle=1;align=center;verticalAlign=middle;", 220, oy, 400, 30)
    v(p+"act", actor, "shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;strokeColor=#000000;fillColor=#ffffff;", 90, oy+60, 30, 60)
    v(p+"view", view, BOX, 400, oy+40, 180, 60)
    v(p+"ctrl", ctrl, BOX, 400, oy+200, 180, 60)
    v(p+"hdl", hdl, BOX, 90, oy+390, 240, 60)
    v(p+"repo", repo, BOX, 630, oy+390, 190, 60)
    e(p+"l1", 135, oy+70, 400, oy+70, False)
    e(p+"l2", 490, oy+100, 490, oy+200, False)
    e(p+"l3", 400, oy+230, 215, oy+390, False)
    e(p+"l4", 330, oy+420, 630, oy+420, False)
    v(p+"m1", m[0], TXT.format(a="center"), 150, oy+40, 250, 20)
    e(p+"a1", 200, oy+63, 330, oy+63, True)
    e(p+"a8", 330, oy+77, 200, oy+77, True)
    v(p+"m8", m[7], TXT.format(a="left"), 155, oy+82, 240, 30)
    e(p+"a2", 505, oy+120, 505, oy+180, True)
    v(p+"m2", m[1], TXT.format(a="left"), 515, oy+140, 300, 20)
    e(p+"a7", 475, oy+180, 475, oy+120, True)
    v(p+"m7", m[6], TXT.format(a="right"), 270, oy+135, 190, 30)
    v(p+"m3", m[2], TXT.format(a="center"), 130, oy+270, 280, 20)
    e(p+"a3", 345, oy+265, 290, oy+310, True)
    e(p+"a6", 315, oy+330, 370, oy+285, True)
    v(p+"m6", m[5], TXT.format(a="left"), 360, oy+305, 280, 30)
    v(p+"m4", m[3], TXT.format(a="center"), 350, oy+390, 280, 20)
    e(p+"a4", 420, oy+413, 530, oy+413, True)
    e(p+"a5", 530, oy+427, 420, oy+427, True)
    v(p+"m5", m[4], TXT.format(a="left"), 425, oy+435, 200, 30)

CR = ":IGenericRepository<Customer>"
SR = ":IGenericRepository<Staff>"
D = [
 ("vpc", "View Profile (Customer)", "Customer", ":ProfileView", ": CustomersController", ":GetCustomerProfileQueryHandler", CR,
  ["1.1 selectViewProfile()", "1.2 getProfile()", "1.3 Handle(query: GetCustomerProfileQuery)",
   "1.4 GetByIdAsync(currentUserId)", "1.5.1 customer: Customer\n1.5.2 null",
   "1.6.1 response (ApiResponse<CustomerProfileDto>)\n1.6.2 errorResponse", "1.7.1 200 OK (ApiResponse)\n1.7.2 404 Not Found",
   "1.8.1 displayProfile(profile)\n1.8.2 displayErrorMessage()"]),
 ("epc", "Edit Profile (Customer)", "Customer", ":EditProfileView", ": CustomersController", ":UpdateCustomerProfileHandler", CR,
  ["1.1 submitUpdateProfile(dto)", "1.2 updateProfile(command: UpdateCustomerProfileCommand)", "1.3 Handle(command: UpdateCustomerProfileCommand)",
   "1.4 GetByIdAsync(id) / Update(customer)", "1.5.1 success: true\n1.5.2 error",
   "1.6.1 response (ApiResponse<CustomerProfileDto>)\n1.6.2 errorResponse", "1.7.1 200 OK (ApiResponse)\n1.7.2 400 Bad Request",
   "1.8.1 displaySuccessMessage()\n1.8.2 displayErrorMessage()"]),
 ("cpc", "Change Password (Customer)", "Customer", ":ChangePasswordView", ": CustomersController", ":ChangePasswordHandler", CR,
  ["1.1 submitChangePassword(oldPwd, newPwd)", "1.2 changePassword(command: ChangePasswordCommand)", "1.3 Handle(command: ChangePasswordCommand)",
   "1.4 GetByIdAsync(id) / Update(customer)", "1.5.1 success: true\n1.5.2 error",
   "1.6.1 response (ApiResponse<bool>)\n1.6.2 errorResponse", "1.7.1 200 OK (ApiResponse)\n1.7.2 400 Bad Request",
   "1.8.1 displaySuccessMessage()\n1.8.2 displayErrorMessage()"]),
 ("vps", "View Profile (Staff / Admin)", "Staff / Admin", ":StaffProfileView", ": StaffsController", ":GetStaffProfileQueryHandler", SR,
  ["1.1 selectViewProfile()", "1.2 getProfile()", "1.3 Handle(query: GetStaffProfileQuery)",
   "1.4 GetByIdAsync(currentUserId, \"Role\")", "1.5.1 staff: Staff\n1.5.2 null",
   "1.6.1 response (ApiResponse<StaffProfileResponseDto>)\n1.6.2 errorResponse", "1.7.1 200 OK (ApiResponse)\n1.7.2 404 Not Found",
   "1.8.1 displayProfile(profile)\n1.8.2 displayErrorMessage()"]),
 ("eps", "Edit Profile (Staff / Admin)", "Staff / Admin", ":EditStaffProfileView", ": StaffsController", ":UpdateStaffProfileCommandHandler", SR,
  ["1.1 submitUpdateProfile(dto)", "1.2 updateProfile(command: UpdateStaffProfileCommand)", "1.3 Handle(command: UpdateStaffProfileCommand)",
   "1.4 GetByIdAsync(id) / Update(staff)", "1.5.1 success: true\n1.5.2 error",
   "1.6.1 response (ApiResponse<StaffProfileResponseDto>)\n1.6.2 errorResponse", "1.7.1 200 OK (ApiResponse)\n1.7.2 400 Bad Request",
   "1.8.1 displaySuccessMessage()\n1.8.2 displayErrorMessage()"]),
 ("cps", "Change Password (Staff / Admin)", "Staff / Admin", ":ChangeStaffPasswordView", ": StaffsController", ":ChangeStaffPasswordCommandHandler", SR,
  ["1.1 submitChangePassword(oldPwd, newPwd)", "1.2 changePassword(command: ChangeStaffPasswordCommand)", "1.3 Handle(command: ChangeStaffPasswordCommand)",
   "1.4 GetByIdAsync(id) / Update(staff)", "1.5.1 success: true\n1.5.2 error",
   "1.6.1 response (ApiResponse<bool>)\n1.6.2 errorResponse", "1.7.1 200 OK (ApiResponse)\n1.7.2 400 Bad Request",
   "1.8.1 displaySuccessMessage()\n1.8.2 displayErrorMessage()"]),
]
for i, d in enumerate(D):
    diagram(d[0] + "_", 30 + i * 520, *d[1:])

xml = ('<mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="827" pageHeight="3200">\n  <root>\n    <mxCell id="0"/>\n    <mxCell id="1" parent="0"/>\n    '
       + "\n    ".join(cells) + "\n  </root>\n</mxGraphModel>\n")
ET.fromstring(xml)
open(__file__.replace("gen_profile.py", "manage_profile.drawio.xml"), "w", encoding="utf-8").write(xml)
print("OK", len(cells))
