-- Create the database if it does not exist and switch to it
CREATE DATABASE IF NOT EXISTS LeavePlanner;
USE LeavePlanner;

-- Create the Countries table with extended Google Calendar codes and names
CREATE TABLE Countries (
    code VARCHAR(100) PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

-- Create the Organizations table without foreign keys
CREATE TABLE Organizations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    workingDays JSON
);

-- Create the Employees table without foreign keys
CREATE TABLE Employees (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    name VARCHAR(255) DEFAULT NULL,
    organization INT,
    managedBy INT,
    country VARCHAR(50),
    isOrgOwner BOOLEAN DEFAULT FALSE,
    paidTimeOff INT DEFAULT 0,
    title VARCHAR(255) DEFAULT NULL
);

-- Create the Leaves table without foreign keys
CREATE TABLE Leaves (
    id INT AUTO_INCREMENT PRIMARY KEY,
    type ENUM('paidTimeOff', 'bankHoliday', 'statutoryLeave'),
    dateStart DATETIME NOT NULL,
    dateEnd DATETIME NOT NULL,
    owner INT,
    description VARCHAR(255),
    approvedBy INT, -- NULL if not approved, employee ID if approved
    rejectedBy INT, -- NULL if not approved, employee ID if approved
    createdAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Add foreign key constraints to Employees
ALTER TABLE Employees
    ADD CONSTRAINT fk_employee_organization FOREIGN KEY (organization) REFERENCES Organizations(id),
    ADD CONSTRAINT fk_employee_managedBy FOREIGN KEY (managedBy) REFERENCES Employees(id);

-- Add foreign key constraints to Leaves
ALTER TABLE Leaves
    ADD CONSTRAINT fk_leave_owner FOREIGN KEY (owner) REFERENCES Employees(id),
    ADD CONSTRAINT fk_leave_approvedBy FOREIGN KEY (approvedBy) REFERENCES Employees(id),
    ADD CONSTRAINT fk_leave_rejectedBy FOREIGN KEY (rejectedBy) REFERENCES Employees(id);


-- Add indexes to commonly queried fields
CREATE INDEX idx_organization ON Employees(organization);
CREATE INDEX idx_owner ON Leaves(owner);

-- Add system employee for the default bank holidays approvals 
INSERT INTO LeavePlanner.Employees (Email, Name, Title, ManagedBy, Country, PaidTimeOff)
VALUES ('system', 'System', 'System Account', NULL, 'None', 0);

-- Populate the Countries table with Google Calendar codes as the code and English names as the name
INSERT INTO Countries (code, name) VALUES
('ad', 'Andorra'),
('ae', 'United Arab Emirates'),
('af', 'Afghanistan'),
('ag', 'Antigua and Barbuda'),
('ai', 'Anguilla'),
('al', 'Albania'),
('am', 'Armenia'),
('ao', 'Angola'),
('ar', 'Argentina'),
('as', 'American Samoa'),
('austrian', 'Austria'),
('australian', 'Australia'),
('aw', 'Aruba'),
('az', 'Azerbaijan'),
('ba', 'Bosnia and Herzegovina'),
('bb', 'Barbados'),
('bd', 'Bangladesh'),
('be', 'Belgium'),
('bf', 'Burkina Faso'),
('bulgarian', 'Bulgaria'),
('bh', 'Bahrain'),
('bi', 'Burundi'),
('bj', 'Benin'),
('bl', 'Saint Barthélemy'),
('bm', 'Bermuda'),
('bn', 'Brunei'),
('bo', 'Bolivia'),
('brazilian', 'Brazil'),
('bs', 'Bahamas'),
('bt', 'Bhutan'),
('bw', 'Botswana'),
('by', 'Belarus'),
('bz', 'Belize'),
('canadian', 'Canada'),
('cd', 'Democratic Republic of the Congo'),
('cf', 'Central African Republic'),
('cg', 'Republic of the Congo'),
('ch', 'Switzerland'),
('ci', 'Côte d''Ivoire'),
('ck', 'Cook Islands'),
('cl', 'Chile'),
('cm', 'Cameroon'),
('china', 'China'),
('co', 'Colombia'),
('cr', 'Costa Rica'),
('cu', 'Cuba'),
('cv', 'Cape Verde'),
('cw', 'Curaçao'),
('cy', 'Cyprus'),
('czech', 'Czech Republic'),
('german', 'Germany'),
('dj', 'Djibouti'),
('danish', 'Denmark'),
('dm', 'Dominica'),
('do', 'Dominican Republic'),
('dz', 'Algeria'),
('ec', 'Ecuador'),
('ee', 'Estonia'),
('eg', 'Egypt'),
('er', 'Eritrea'),
('spain', 'Spain'),
('et', 'Ethiopia'),
('finnish', 'Finland'),
('fj', 'Fiji'),
('fk', 'Falkland Islands'),
('fm', 'Federated States of Micronesia'),
('fo', 'Faroe Islands'),
('french', 'France'),
('ga', 'Gabon'),
('uk', 'United Kingdom'),
('gd', 'Grenada'),
('ge', 'Georgia'),
('gf', 'French Guiana'),
('gg', 'Guernsey'),
('gh', 'Ghana'),
('gi', 'Gibraltar'),
('gl', 'Greenland'),
('gm', 'The Gambia'),
('gn', 'Guinea'),
('gp', 'Guadeloupe'),
('gq', 'Equatorial Guinea'),
('greek', 'Greece'),
('gt', 'Guatemala'),
('gu', 'Guam'),
('gw', 'Guinea-Bissau'),
('gy', 'Guyana'),
('hong_kong', 'Hong Kong'),
('hn', 'Honduras'),
('croatian', 'Croatia'),
('ht', 'Haiti'),
('hungarian', 'Hungary'),
('indonesian', 'Indonesia'),
('irish', 'Ireland'),
('jewish', 'Israel'),
('im', 'Isle of Man'),
('indian', 'India'),
('iq', 'Iraq'),
('ir', 'Iran'),
('is', 'Iceland'),
('italian', 'Italy'),
('je', 'Jersey'),
('jm', 'Jamaica'),
('jo', 'Jordan'),
('japanese', 'Japan'),
('ke', 'Kenya'),
('kg', 'Kyrgyzstan'),
('kh', 'Cambodia'),
('ki', 'Kiribati'),
('km', 'Comoros'),
('kn', 'Saint Kitts and Nevis'),
('kp', 'North Korea'),
('south_korea', 'South Korea'),
('kw', 'Kuwait'),
('ky', 'Cayman Islands'),
('kz', 'Kazakhstan'),
('la', 'Laos'),
('lb', 'Lebanon'),
('lc', 'Saint Lucia'),
('li', 'Liechtenstein'),
('lk', 'Sri Lanka'),
('lr', 'Liberia'),
('ls', 'Lesotho'),
('lithuanian', 'Lithuania'),
('lu', 'Luxembourg'),
('latvian', 'Latvia'),
('ly', 'Libya'),
('ma', 'Morocco'),
('mc', 'Monaco'),
('md', 'Moldova'),
('me', 'Montenegro'),
('mf', 'Saint Martin'),
('mg', 'Madagascar'),
('mh', 'Marshall Islands'),
('mk', 'North Macedonia'),
('ml', 'Mali'),
('mm', 'Myanmar'),
('mn', 'Mongolia'),
('mo', 'Macau'),
('mp', 'Northern Mariana Islands'),
('mq', 'Martinique'),
('mr', 'Mauritania'),
('ms', 'Montserrat'),
('mt', 'Malta'),
('mu', 'Mauritius'),
('mv', 'Maldives'),
('mw', 'Malawi'),
('mexican', 'Mexico'),
('malaysia', 'Malaysia'),
('mz', 'Mozambique'),
('na', 'Namibia'),
('nc', 'New Caledonia'),
('ne', 'Niger'),
('ng', 'Nigeria'),
('ni', 'Nicaragua'),
('dutch', 'Netherlands'),
('norwegian', 'Norway'),
('np', 'Nepal'),
('nr', 'Nauru'),
('new_zealand', 'New Zealand'),
('om', 'Oman'),
('pa', 'Panama'),
('pe', 'Peru'),
('pf', 'French Polynesia'),
('pg', 'Papua New Guinea'),
('philippines', 'Philippines'),
('pk', 'Pakistan'),
('polish', 'Poland'),
('pm', 'Saint Pierre and Miquelon'),
('pr', 'Puerto Rico'),
('portuguese', 'Portugal'),
('pw', 'Palau'),
('py', 'Paraguay'),
('qa', 'Qatar'),
('re', 'Réunion'),
('romanian', 'Romania'),
('rs', 'Serbia'),
('russian', 'Russia'),
('rw', 'Rwanda'),
('saudiarabian', 'Saudi Arabia'),
('sb', 'Solomon Islands'),
('sc', 'Seychelles'),
('sd', 'Sudan'),
('swedish', 'Sweden'),
('singapore', 'Singapore'),
('sh', 'Saint Helena'),
('slovenian', 'Slovenia'),
('slovak', 'Slovakia'),
('sl', 'Sierra Leone'),
('sm', 'San Marino'),
('sn', 'Senegal'),
('so', 'Somalia'),
('sr', 'Suriname'),
('ss', 'South Sudan'),
('st', 'São Tomé and Príncipe'),
('sv', 'El Salvador'),
('sx', 'Sint Maarten'),
('sy', 'Syria'),
('sz', 'Eswatini'),
('tc', 'Turks and Caicos Islands'),
('td', 'Chad'),
('tg', 'Togo'),
('th', 'Thailand'),
('tj', 'Tajikistan'),
('tl', 'Timor-Leste'),
('tm', 'Turkmenistan'),
('tn', 'Tunisia'),
('to', 'Tonga'),
('turkish', 'Turkey'),
('tt', 'Trinidad and Tobago'),
('tv', 'Tuvalu'),
('taiwan', 'Taiwan'),
('tz', 'Tanzania'),
('ukrainian', 'Ukraine'),
('ug', 'Uganda'),
('usa', 'United States'),
('uy', 'Uruguay'),
('uz', 'Uzbekistan'),
('va', 'Vatican City'),
('vc', 'Saint Vincent and the Grenadines'),
('ve', 'Venezuela'),
('vg', 'British Virgin Islands'),
('vi', 'U.S. Virgin Islands'),
('vietnamese', 'Vietnam'),
('vu', 'Vanuatu'),
('wf', 'Wallis and Futuna'),
('ws', 'Samoa'),
('xk', 'Kosovo'),
('ye', 'Yemen'),
('yt', 'Mayotte'),
('sa', 'South Africa'),
('zm', 'Zambia'),
('zw', 'Zimbabwe');
