import React, { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Checkbox, Form, Icon, Input, Popup } from 'semantic-ui-react';

const Bool = ({ value, sort, name, setSort, text, onChange, emptyValue }) => {

    const items = useMemo(() => {
        let obj = [
            {
                text: 'Да',
                value: true,
            },
            {
                text: 'Нет',
                value: false,
            },
        ];

        if (emptyValue === 'allowed') {
            obj.push({
                text: 'Не выбрано',
                value: 'null',
            });
        }

        return obj;
    }, [emptyValue]);

    const { t } = useTranslation();

    const handleChange = newValue => {
        if (values.some(x => x === newValue)) {
            values.splice(values.indexOf(newValue), 1);
        } else {
            values.push(newValue);
        }
        if (onChange !== undefined) onChange(null, { name: name, value: values.join('|') });
    };

    let values = (value ? value.split('|') : []).map(item =>
        item === 'true' ? true : item === 'false' ? false : 'null',
    );

    let content = (
        <Form>
            {/* <label className="label-in-popup">{t(name)}</label>*/}
            {/*  <div className="boolean-facet-values">*/}
            {items &&
                items.map(x => {
                    return (
                        <Form.Field key={x.text}>
                            <Checkbox
                                label={x.text}
                                name="checkboxRadioGroup"
                                value={x.value}
                                checked={values.includes(x.value)}
                                onChange={() => handleChange(x.value)}
                            />
                        </Form.Field>
                    );
                })}
            {/*</div>*/}
        </Form>
    );

    return (
        <div className="facet-input">
            {content}
            {/* <Popup
                trigger={
                    <Input
                        fluid
                        label={{ basic: true, content: '' }}
                        labelPosition="right"
                        onKeyPress={e => {
                            e.preventDefault();
                        }}
                        placeholder={
                            value !== undefined
                                ? items.find(item => item.value === value) &&
                                  items.find(item => item.value === value).text
                                : t(name)
                        }
                    />
                }
                content={content}
                on="click"
                hoverable
                className="from-popup"
                position="bottom left"
            />
            <Button
                className={`sort-button sort-button-up ${
                    sort === 'asc' ? 'sort-button-active' : ''
                }`}
                name={name}
                value="asc"
                onClick={setSort}
            >
                <Icon name="caret up" />
            </Button>
            <Button
                className={`sort-button sort-button-down ${
                    sort === 'desc' ? 'sort-button-active' : ''
                }`}
                name={name}
                value="desc"
                onClick={setSort}
            >
                <Icon name="caret down" />
            </Button>*/}
        </div>
    );
};

export default Bool;
