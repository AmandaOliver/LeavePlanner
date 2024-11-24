import {
  Modal,
  ModalContent,
  ModalBody,
  ModalFooter,
  Button,
  ModalHeader,
  Input,
  Table,
  TableBody,
  TableCell,
  TableColumn,
  TableHeader,
  TableRow,
  Tooltip,
  Code,
  Card,
} from '@nextui-org/react'
import { useState } from 'react'
import { useOrganizationModel } from '../models/Organization'
import { InfoIcon } from '../icons/info'
export const ImportModal = ({
  isOpen,
  onOpenChange,
  onCloseCb,
}: {
  isOpen: boolean
  onOpenChange: () => void
  onCloseCb: () => void
}) => {
  const [isLoading, setIsLoading] = useState(false)
  const { importOrganization } = useOrganizationModel()
  const [file, setFile] = useState<File | null>(null)
  const [message, setMessage] = useState<string | null>('')
  const [error, setError] = useState<string | null>('')
  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = event.target.files?.[0] || null
    setFile(selectedFile)
    setMessage(null)
  }
  const handleFileBlur = () => {
    if (!file) {
      setMessage('Please select a CSV file.')
    } else {
      setMessage(null)
    }
  }
  const importHandler = async (onClose: () => void) => {
    setIsLoading(true)
    if (file) {
      const formData = new FormData()
      formData.append('file', file)
      try {
        await importOrganization(formData)
        setIsLoading(false)
        onCloseCb()
        onClose()
      } catch (error: any) {
        setError(error.message)
        setIsLoading(false)
      }
    }
  }
  return (
    <Modal
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      onClose={onCloseCb}
      size="2xl"
    >
      <ModalContent>
        {(onClose) => (
          <>
            <ModalHeader className="flex flex-col gap-1">
              Import Organization Tree
            </ModalHeader>

            <p className="m-auto">
              Select a .csv file with the following structure.
            </p>
            <ModalBody>
              <Table aria-label="Example csv" hideHeader>
                <TableHeader>
                  <TableColumn>hidden</TableColumn>
                  <TableColumn>hidden</TableColumn>
                  <TableColumn>hidden</TableColumn>
                  <TableColumn>hidden</TableColumn>
                  <TableColumn>hidden</TableColumn>
                  <TableColumn>hidden</TableColumn>
                  <TableColumn>hidden</TableColumn>
                </TableHeader>
                <TableBody>
                  <TableRow key="1">
                    <TableCell className="min-w-24">
                      <Tooltip
                        content="Required, must have valid format."
                        color="primary"
                      >
                        <Code className="flex flex-wrap flex-row items-center gap-2 rounded-md">
                          <InfoIcon />
                          Email
                        </Code>
                      </Tooltip>
                    </TableCell>
                    <TableCell className="min-w-28">
                      <Tooltip content="Required." color="primary">
                        <Code className="flex flex-wrap flex-row items-center gap-2 rounded-md">
                          <InfoIcon />
                          Name
                        </Code>
                      </Tooltip>
                    </TableCell>
                    <TableCell className="min-w-28">
                      <Tooltip content="Required." color="primary">
                        <Code className="flex flex-wrap flex-row items-center gap-2 rounded-md">
                          <InfoIcon />
                          Title
                        </Code>
                      </Tooltip>
                    </TableCell>
                    <TableCell className="min-w-44">
                      <Tooltip
                        content="Optional. Must have valid format."
                        color="primary"
                      >
                        <Code className="flex flex-wrap flex-row items-center gap-2 rounded-md">
                          <InfoIcon />
                          ManagerEmail
                        </Code>
                      </Tooltip>
                    </TableCell>
                    <TableCell className="min-w-32">
                      <Tooltip
                        content={
                          <>
                            Required. Must be one of the list: Andorra,
                            Afghanistan, United Arab Emirates, Antigua and
                            Barbuda, Anguilla, Albania, Armenia, Angola,
                            Argentina, American Samoa, an', 'Austria, lian',
                            'Australia, Aruba, Azerbaijan, Bosnia and
                            Herzegovina, Barbados, Bangladesh, Belgium, Burkina
                            Faso, ian', 'Bulgaria, Bahrain, Burundi, Benin,
                            Saint Barthélemy, Bermuda, Brunei, Bolivia, Brazil,
                            Bahamas, Bhutan, Botswana, Belarus, Belize, Canada,
                            Democratic Republic of the Congo, Central African
                            Republic, Republic of the Congo, Switzerland, Côte
                            d''Ivoire, Cook Islands, Chile, Cameroon, China,
                            Colombia, Costa Rica, Cuba, Cape Verde, Curaçao,
                            Cyprus, Czech Republic, Germany, Djibouti, Denmark,
                            Dominica, Dominican Republic, Algeria, Ecuador,
                            Estonia, Egypt, Eritrea, Spain, Ethiopia, Finland,
                            Fiji, Falkland Islands, Federated States of
                            Micronesia, Faroe Islands, France, Gabon, United
                            Kingdom, Grenada, Georgia, French Guiana, Guernsey,
                            Ghana, Gibraltar, Greenland, The Gambia, Guinea,
                            Guadeloupe, Equatorial Guinea, Greece, Guatemala,
                            Guam, Guinea-Bissau, Guyana, Hong Kong, Honduras,
                            Croatia, Haiti, Hungary, Indonesia, Ireland, Israel,
                            Isle of Man, India, Iraq, Iran, Iceland, Italy,
                            Jersey, Jamaica, Jordan, Japan, Kenya, Kyrgyzstan,
                            Cambodia, Kiribati, Comoros, Saint Kitts and Nevis,
                            North Korea, South Korea, Kuwait, Cayman Islands,
                            Kazakhstan, Laos, Lebanon, Saint Lucia,
                            Liechtenstein, Sri Lanka, Liberia, Lesotho,
                            Lithuania, Luxembourg, Latvia, Libya, Morocco,
                            Monaco, Moldova, Montenegro, Saint Martin,
                            Madagascar, Marshall Islands, North Macedonia, Mali,
                            Myanmar, Mongolia, Macau, Northern Mariana Islands,
                            Martinique, Mauritania, Montserrat, Malta,
                            Mauritius, Maldives, Malawi, Mexico, Malaysia,
                            Mozambique, Namibia, New Caledonia, Niger, Nigeria,
                            Nicaragua, Netherlands, Norway, Nepal, Nauru, New
                            Zealand, Oman, Panama, Peru, French Polynesia, Papua
                            New Guinea, Philippines, Pakistan, Poland, Saint
                            Pierre and Miquelon, Puerto Rico, uese', 'Portugal,
                            Palau, Paraguay, Qatar, Réunion, Romania, Serbia,
                            Russia, Rwanda, Saudi Arabia, Solomon Islands,
                            Seychelles, Sudan, Sweden, Singapore, Saint Helena,
                            Slovenia, Slovakia, Sierra Leone, San Marino,
                            Senegal, Somalia, Suriname, South Sudan, São Tomé
                            and Príncipe, El Salvador, Sint Maarten, Syria,
                            Eswatini, Turks and Caicos Islands, Chad, Togo,
                            Thailand, Tajikistan, Timor-Leste, Turkmenistan,
                            Tunisia, Tonga, Turkey, Trinidad and Tobago, Tuvalu,
                            Taiwan, Tanzania, Ukraine, Uganda, United States,
                            Uruguay, Uzbekistan, Vatican City, Saint Vincent and
                            the Grenadines, Venezuela, British Virgin Islands,
                            U.S. Virgin Islands, Vietnam, Vanuatu, Wallis and
                            Futuna, Samoa, Kosovo, Yemen, Mayotte, South Africa,
                            Zambia, Zimbabwe.
                          </>
                        }
                        color="primary"
                      >
                        <Code className="flex flex-wrap flex-row items-center gap-2 rounded-md">
                          <InfoIcon />
                          Country
                        </Code>
                      </Tooltip>
                    </TableCell>
                    <TableCell className="min-w-40">
                      <Tooltip
                        content="Required. Integer number higher than 1"
                        color="primary"
                      >
                        <Code className="flex flex-wrap flex-row items-center gap-2 rounded-md">
                          <InfoIcon />
                          PaidTimeOff
                        </Code>
                      </Tooltip>
                    </TableCell>
                    <TableCell className="min-w-40">
                      <Tooltip
                        content="Required. true or false"
                        color="primary"
                      >
                        <Code className="flex flex-wrap flex-row items-center gap-2 rounded-md">
                          <InfoIcon />
                          IsAdmin
                        </Code>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                  <TableRow key="2">
                    <TableCell>name@email.com</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>CEO</TableCell>
                    <TableCell>manager@email.com</TableCell>
                    <TableCell>Spain</TableCell>
                    <TableCell>30</TableCell>
                    <TableCell>false</TableCell>
                  </TableRow>
                </TableBody>
              </Table>
              {error && (
                <Card className="bg-danger p-4 text-white ">
                  <p>{error}</p>
                </Card>
              )}
              <Input
                type="file"
                accept=".csv"
                onChange={handleFileChange}
                onBlur={handleFileBlur}
                label="Import .csv"
                isInvalid={!!message}
                errorMessage={message}
              />
            </ModalBody>
            <ModalFooter>
              <Button variant="light" onPress={onClose}>
                Close
              </Button>
              <Button
                color="primary"
                onPress={() => importHandler(onClose)}
                isLoading={isLoading}
                isDisabled={!file}
              >
                Import
              </Button>
            </ModalFooter>
          </>
        )}
      </ModalContent>
    </Modal>
  )
}
